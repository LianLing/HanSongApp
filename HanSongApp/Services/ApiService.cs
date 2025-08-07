using HanSongApp.Models.HtsModels;
using HanSongApp.Views;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;




namespace HanSongApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConnectivity _connectivity;

        public ApiService(HttpClient httpClient, IConnectivity connectivity)
        {

            // 使用MAUI的Android客户端处理程序
            _connectivity = connectivity;
            _httpClient = new HttpClient();
            // 强制使用 HTTP/2
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                //var handler = new AndroidMessageHandler();
                //handler.Properties["http.allowCleartext"] = true; // 关键：允许明文HTTP

                _httpClient.DefaultRequestVersion = HttpVersion.Version20;
                _httpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
            }

            // 从用户设置获取Token
            var token = Preferences.Default.Get("api_token", string.Empty);
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            // 设置基础URL（生产环境/开发环境切换）
            _httpClient.BaseAddress = new Uri("http://10.10.38.158:8201/htsapi/db1v0/");

            // 设置默认超时时间
            _httpClient.Timeout = TimeSpan.FromSeconds(20);

            //try
            //{
            //    var testClient = new HttpClient();
            //    var response = testClient.GetAsync("http://10.10.38.158").GetAwaiter().GetResult();
            //    Console.WriteLine($"HTTP测试成功: {response.StatusCode}");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"HTTP测试失败: {ex.Message}");
            //}
        }

        // 新增：HTS API端点
        private const string ChkInEndpoint = "chk_in";
        private const string ChkOutEndpoint = "chk_out";
        private const string GetSnInfoEndpoint = "get_sn_info";

        /// <summary>
        /// 执行入站检查 (chk_in)
        /// </summary>
        public async Task<ApiResponse<Ack>> ChkInAsync(ChkInReq request)
        {
            var response =  await PostAsync<ChkInReq,Ack>(ChkInEndpoint, request, retryOnFailure: true);
            return response;
        }
        

        /// <summary>
        /// 上报测试结果 (chk_out)
        /// </summary>
        public async Task<ApiResponse<Ack>> ChkOutAsync(ChkOutReq request)
        {
            return await PostAsync<ChkOutReq, Ack>(ChkOutEndpoint, request, retryOnFailure: true);
        }

        /// <summary>
        /// 查询SN信息 (get_sn_info)
        /// </summary>
        public async Task<ApiResponse<Ack>> GetSnInfoAsync(SnInfoReq request)
        {
            return await PostAsync<SnInfoReq, Ack>(GetSnInfoEndpoint, request, retryOnFailure: true);
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        public async Task<ApiResponse<LoginResponse>> LoginAsync(string username, string password)
        {
            if (!IsNetworkAvailable())
                return ApiResponse<LoginResponse>.NoNetwork();

            try
            {
                var request = new LoginRequest
                {
                    Username = username,
                    Password = password
                };

                return await PostAsync<LoginRequest, LoginResponse>("auth/login", request);
            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResponse>.Error($"登录失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 设置访问令牌
        /// </summary>
        public void SetAccessToken(string token)
        {
            Preferences.Default.Set("api_token", token);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// 通用GET请求
        /// </summary>
        public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
        {
            if (!IsNetworkAvailable())
            {
                return ApiResponse<T>.NoNetwork();
            }

            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                return await ParseResponse<T>(response);
            }
            catch (TaskCanceledException) // 超时
            {
                return ApiResponse<T>.Error("请求超时，请检查网络连接");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"API请求异常: {ex.Message}");
                return ApiResponse<T>.Error($"请求失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 通用POST请求
        /// </summary>
        public async Task<ApiResponse<TResult>> PostAsync<TData, TResult>(
            string endpoint,
            TData data,
            bool retryOnFailure = false,
            int maxRetries = 2)
        {
            if (!IsNetworkAvailable())
            {
                return ApiResponse<TResult>.NoNetwork();
            }

            int retryCount = 0;
            while (retryCount <= maxRetries)
            {
                try
                {
                    string s = JsonSerializer.Serialize(data);
                    using var content = new StringContent(
                        s,
                        Encoding.UTF8,
                        "application/json");

                    var response = await _httpClient.PostAsync(endpoint, content);
                    // 读取响应内容为字符串
                    //string msg = await response.Content.ReadAsStringAsync();
                    //var result = JsonSerializer.Deserialize<Ack>(msg);
                    var apiResponse = await ParseResponse<TResult>(response);

                    // 成功或致命错误直接返回
                    if (apiResponse.IfSuccess || !retryOnFailure ||
                        apiResponse.StatusCode >= 500 || apiResponse.StatusCode == 401)
                    {
                        return apiResponse;
                    }

                    // 可重试错误
                    if (retryOnFailure && !apiResponse.IfSuccess)
                    {
                        retryCount++;
                        await Task.Delay(1000 * retryCount); // 指数退避
                    }
                }
                catch (TaskCanceledException)
                {
                    retryCount++;
                    if (retryCount > maxRetries)
                    {
                        return ApiResponse<TResult>.Error("请求超时，请检查网络连接");
                    }
                    await Task.Delay(1000 * retryCount);
                }
                catch (HttpRequestException ex) when (ex.Message.Contains("Cleartext"))
                {
                    // 特定处理明文流量错误
                    return ApiResponse<TResult>.Error(
                        "网络安全错误：请检查Android安全配置",
                        495,  // 自定义错误码
                        "CLEARTEXT_NOT_ALLOWED");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"API请求异常: {ex.Message}");
                    return ApiResponse<TResult>.Error($"请求失败: {ex.Message}");
                }
            }

            return ApiResponse<TResult>.Error("请求失败，请重试");
        }

        /// <summary>
        /// 解析API响应
        /// </summary>
        private async Task<ApiResponse<T>> ParseResponse<T>(HttpResponseMessage response)
        {
            var statusCode = (int)response.StatusCode;

            // 处理HTTP错误状态
            if (!response.IsSuccessStatusCode)
            {
                return await HandleApiError<T>(response, statusCode);
            }

            try
            {
                // 尝试解析JSON响应
                var result = await response.Content.ReadFromJsonAsync<T>(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return ApiResponse<T>.Success(result, statusCode);
            }
            catch (JsonException)
            {
                // 解析失败，尝试读取原始错误信息
                var errorContent = await response.Content.ReadAsStringAsync();
                return ApiResponse<T>.Error($"响应解析失败: {errorContent?.Substring(100)}", statusCode);
            }
        }

        /// <summary>
        /// 处理API错误
        /// </summary>
        private async Task<ApiResponse<T>> HandleApiError<T>(
            HttpResponseMessage response,
            int statusCode)
        {
            try
            {
                // 尝试解析错误响应
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return ApiResponse<T>.Error(
                    error?.Message ?? $"API返回错误 ({statusCode})",
                    statusCode,
                    error?.ErrorCode);
            }
            catch
            {
                // 无法解析JSON，直接显示内容
                var errorContent = await response.Content.ReadAsStringAsync();
                return ApiResponse<T>.Error($"服务器错误: {statusCode} - {errorContent?.Substring(100)}", statusCode);
            }
        }

        /// <summary>
        /// 检查网络连接
        /// </summary>
        public bool IsNetworkAvailable()
        {
            if (_connectivity.NetworkAccess != Microsoft.Maui.Networking.NetworkAccess.Internet)
            {
                // 记录离线状态
                return false;
            }
            return true;
        }

        
    }

     

    /// <summary>
    /// API响应封装类
    /// </summary>
    public class ApiResponse<T>
    {
        public bool IfSuccess { get; }
        public T Data { get; }
        public string ErrorMessage { get; }
        public int StatusCode { get; }
        public string ErrorCode { get; }

        private ApiResponse(bool success, T data, string errorMessage, int statusCode, string errorCode = null)
        {
            IfSuccess = success;
            Data = data;
            ErrorMessage = errorMessage;
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }

        public static ApiResponse<T> Success(T data, int statusCode = 200)
            => new ApiResponse<T>(true, data, null, statusCode);

        public static ApiResponse<T> Error(string errorMessage, int statusCode = 500, string errorCode = null)
            => new ApiResponse<T>(false, default, errorMessage, statusCode, errorCode);

        public static ApiResponse<T> NoNetwork()
            => new ApiResponse<T>(false, default, "网络不可用，请检查连接", 0, "NETWORK_UNAVAILABLE");
    }

    /// <summary>
    /// API错误响应结构
    /// </summary>
    public class ApiErrorResponse
    {
        [JsonPropertyName("error")]
        public string Error { get; set; }

        [JsonPropertyName("error_code")]
        public string ErrorCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("details")]
        public Dictionary<string, string> Details { get; set; }
    }
}
