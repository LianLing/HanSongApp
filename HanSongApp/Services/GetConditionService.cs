using HanSongApp.DataBase;
using HanSongApp.Models;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanSongApp.Services
{
    public class GetConditionService : IDisposable
    {
        private readonly DbContext _db;
        public GetConditionService() => _db = new DbContext();

        public void Dispose() => _db?.Dispose();

        /// <summary>
        /// 获取机型列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<Prod_TypeModel>> OnProdTypeSelected()
        {
            try
            {
                string sql = $@"SELECT t.`code`,t.name FROM prod_type t WHERE t.CODE > 'A001' ORDER BY t.name";
                var machineKinds = await _db.Instance.Ado.SqlQueryAsync<Prod_TypeModel>(sql);

                return machineKinds;
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// 获取工序列表
        /// </summary>
        /// <param name="prod_type"></param>
        /// <returns></returns>
        public async Task<List<modelModel>> GetModelList(string prod_type)
        {
            try
            {
                //_db.Instance.CurrentConnectionConfig.ConnectionString = $@"Server=10.10.1.80;Port=3306;Database=hts_prod_{prod_type};Uid=htsusr;Pwd=HtsUsr.1;Connect Timeout=10;CharSet=utf8mb4;Pooling=false;";
                string sql = $@"SELECT t.prod_type,t.prod_model,t.prod_module FROM cfg_model t WHERE prod_type = '{prod_type}' ORDER BY t.prod_model";
                var modules = await _db.Instance.Ado.SqlQueryAsync<modelModel>(sql);
                return modules;
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 获取制程列表
        /// </summary>
        /// <param name="prod_type"></param>
        /// <returns></returns>
        public async Task<List<string>> GetProcessList(string prod_type)
        {
            try
            {
                string sql = $@"SELECT distinct t.prod_process FROM cfg_process t WHERE prod_type = '{prod_type}' ORDER BY prod_process";
                var processes = await _db.Instance.Ado.SqlQueryAsync<string>(sql);
                return processes;
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 获取线别列表
        /// </summary>
        /// <param name="prod_type"></param>
        /// <returns></returns>
        public async Task<List<LineModel>> GetLineList()
        {
            try
            {
                string sql = $@"select t.`code`,t.`name` from prod_line t ORDER BY `code` ";
                var lines = await _db.Instance.Ado.SqlQueryAsync<LineModel>(sql);
                return lines;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取班组列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<TeamModel>> GetClassTeamList()
        {
            try
            {
                string sql = $@"select t.`code`,t.`name` from prod_team t ORDER BY `code` ";
                var teams = await _db.Instance.Ado.SqlQueryAsync<TeamModel>(sql);
                return teams;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<string>> GetMoList(string teamcode)
        {
            try
            {
                var sqlServerConfig = new ConnectionConfig()
                {
                    ConnectionString = $@"Data Source=10.10.1.146; User ID=hts; Password=adminhts; Initial Catalog=HS_MES_PROD;Connect Timeout=30;Encrypt=False",
                    DbType = DbType.SqlServer,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute
                };

                DateTime todayStart = DateTime.Today; // 今天0点
                DateTime tomorrowEnd = todayStart.AddDays(2).AddSeconds(-1); // 明天23:59
                string startTime = todayStart.ToString("yyyy-MM-dd HH:mm:ss");
                string endTime = tomorrowEnd.ToString("yyyy-MM-dd HH:mm:ss");


                string sql = $@"select t.MO+','+t.PartNo+','+t.OrderNO+','+t.PartName+','+t.STATUS as Mo from MES_MO_PLAN t where t.TeamCode = '{teamcode}' and t.PlanEndTime BETWEEN {startTime} AND {endTime}";
                using (var sqlServerDb = new SqlSugarClient(sqlServerConfig))
                {
                    var mo = await sqlServerDb.Ado.SqlQueryAsync<string>(sql);
                    return mo;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
