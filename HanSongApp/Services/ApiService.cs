using HanSongApp.Models;
using HanSongApp.Views;
using Microsoft.Maui.Storage;
using System.Diagnostics;
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
            _httpClient = httpClient;
            _connectivity = connectivity;

            // 从用户设置获取Token
            var token = Preferences.Default.Get("api_token", string.Empty);
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            // 设置基础URL（生产环境/开发环境切换）
#if DEBUG
            _httpClient.BaseAddress = new Uri("http://10.10.38.158:8201/htsapi/db1v0/");
#else
        _httpClient.BaseAddress = new Uri("https://api.industrial.com/v1");
#endif

            // 设置默认超时时间
            _httpClient.Timeout = TimeSpan.FromSeconds(20);
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
                    using var content = new StringContent(
                        JsonSerializer.Serialize(data),
                        Encoding.UTF8,
                        "application/json");

                    var response = await _httpClient.PostAsync(endpoint, content);
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
        private bool IsNetworkAvailable()
        {
            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                // 记录离线状态
                return false;
            }
            return true;
        }

        //public async Task<List<SnInfo>> GetStations(SnInfo input, string type_code)
        //{
        //    if (!IsNetworkAvailable())
        //        return new List<SnInfo>(); // 修复：返回一个空的 List<f> 而不是 ApiResponse<SnInfo>

        //    try
        //    {
        //        var request = new SnInfo
        //        {
        //            // 根据需要填充 request 对象
        //        };

        //        var response = await PostAsync<SnInfo, List<SnInfo>>("stations/get", request); 
        //        if (response.IfSuccess)
        //        {
        //            return response.Data; // 返回成功的结果
        //        }
        //        else
        //        {
        //            Debug.WriteLine($"获取站点失败: {response.ErrorMessage}");
        //            return new List<SnInfo>(); // 返回空列表以处理失败情况
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"获取站点异常: {ex.Message}");
        //        return new List<SnInfo>(); // 返回空列表以处理异常情况
        //    }
        //}
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
