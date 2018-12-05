using ConsoleApp1.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("==========程式開始==========");

                DateTime dt = DateTime.Now;

                #region 設定連線字串
                DefaultConn = System.Configuration.ConfigurationManager.
                    ConnectionStrings["DefaultConnection"].ConnectionString;
                #endregion              

                #region 檢查有無DataBase 如無則新增DataBase
                //檢查有無DataBase 如無則建立DataBase
                CheckDataBase();
                #endregion

                #region 檢查有無DataTable(Users) 如無則新增DataTable(Users)
                //檢查有無DataTable 如無則建立DataTable
                CheckDataTable();
                #endregion               

                #region ADO 查詢
                SelectUserByAdoNet();
                #endregion

                #region Dapper 查詢
                //int[] searchDatas = null;
                //searchDatas = new int[] { 1002, 1003, 1007 };
                //SelectUserByDapper(searchDatas);
                #endregion

                #region 使用 Dynamic 查詢
                //SelectUseDynamic();
                #endregion

                #region Dapper 新增
                //新增多筆資料
                //List<Users> datas = new List<Users>() {
                //    new Users(){
                //        Email = "aaa@gmail.com",
                //        Name = "小明",
                //        IsEnable = true,
                //        GuidID = Guid.NewGuid(),
                //        Money = 1234M,
                //        LastUpdateTime = dt,
                //    },
                //    new Users(){
                //        Name = "小華",
                //        IsEnable = false,
                //        GuidID = Guid.NewGuid(),
                //        Money = 300M,
                //        LastUpdateTime = dt,
                //    },
                //    new Users(){
                //        Name = "小美",
                //        IsEnable = false,
                //        GuidID = Guid.NewGuid(),
                //        Money = 1500M,
                //        LastUpdateTime = dt,
                //    }
                //};
                //InsertUsers(datas);
                #endregion

                #region Dapper 修改
                //修改多筆資料
                //List<Users> datas = new List<Users>() {
                //    new Users(){
                //        UserId = 1,
                //        Email = "bbb@gmail.com",
                //        Name = "小雄",
                //        IsEnable = false,
                //        GuidID = Guid.NewGuid(),
                //        Money = 50.23M,
                //        LastUpdateTime = dt,
                //    },
                //    new Users(){
                //        UserId = 3,
                //        Name = "小芳",
                //        IsEnable = true,
                //        GuidID = Guid.NewGuid(),
                //        Money = 1300M,
                //        LastUpdateTime = dt,
                //    }
                //};
                //UpdateUsers(datas);
                #endregion

                #region Dapper 刪除
                //刪除多筆資料
                //List<Users> datas = new List<Users>() {
                //    new Users(){
                //        UserId = 1,
                //    },
                //    new Users(){
                //        UserId = 2,
                //    }
                //};
                //DeleteUsers(datas);
                #endregion

                #region Dapper Transaction
                //Transaction();
                #endregion

                #region Dapper TransactionScope
                //TransactionScope();
                #endregion

            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("錯誤:{0}", ex.Message));
                //throw ex;
            }
            finally
            {
                Console.WriteLine(string.Empty);
                Console.WriteLine("==========程式結束==========");
                Console.ReadLine();
            }
        }

        #region 前置動作 & 預設

        /// <summary>
        /// 連線字串
        /// </summary>
        private static string DefaultConn = string.Empty;

        /// <summary>
        /// 檢查有無DataBase 如無則建立DataBase
        /// </summary>
        private static void CheckDataBase()
        {
            Database.SetInitializer<MyContext>(null);
            using (var context = new MyContext())
            {
                if (!context.Database.Exists())
                    ((IObjectContextAdapter)context).ObjectContext.CreateDatabase();
            }
        }

        /// <summary>
        /// 檢查有無DataTable(Users) 如無則新增DataTable(Users)
        /// </summary>
        private static void CheckDataTable()
        {
            using (SqlConnection conn = new SqlConnection(DefaultConn))
            {
                conn.Open();
                string commandStr = @"If not exists (select name from sysobjects where name = 'Users') 
CREATE TABLE Users(UserId int IDENTITY(1,1) not null, Email nvarchar(max),Name nvarchar(50) not null,IsEnable bit,GuidID uniqueidentifier,Money decimal(18,2) ,LastUpdateTime datetime
CONSTRAINT PK_Users PRIMARY KEY([UserId]))";

                using (SqlCommand command = new SqlCommand(commandStr, conn))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region CRUD

        /// <summary>
        /// 查詢資料 By ADO.NET
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Users> SelectUserByAdoNet()
        {
            Console.WriteLine(string.Empty);
            Console.WriteLine("----------使用ADO.NET查詢----------");

            List<Users> results = new List<Users>();

            using (SqlConnection conn = new SqlConnection(DefaultConn))
            {
                conn.Open();
                string strSql = @"
select [UserId]
      ,[Email]
      ,[Name]
      ,[IsEnable]
      ,[GuidID]
      ,[Money]
      ,[LastUpdateTime] from Users ;";
                using (SqlCommand com = new SqlCommand(strSql, conn))
                {
                    SqlDataReader reader = com.ExecuteReader();
                    while (reader.Read())
                    {
                        Nullable<Guid> ng = null;
                        Guid g = new Guid();

                        Users user = new Users()
                        {
                            UserId = Convert.ToInt32(reader["UserId"]),
                            Email = Convert.ToString(reader["Email"]),
                            Name = Convert.ToString(reader["Name"]),
                            IsEnable = Convert.ToBoolean(reader["IsEnable"]),
                            //GuidID = (Guid.TryParse(reader["GuidID"]?.ToString(), out g)) ? g : ng,
                            GuidID = (Guid.TryParse(((reader["GuidID"] == null) ? null : reader["GuidID"].ToString()), out g)) ? g : ng,
                            Money = Convert.ToDecimal(reader["Money"]),
                            LastUpdateTime = Convert.ToDateTime(reader["LastUpdateTime"]),
                        };
                        results.Add(user);
                    }
                    reader.Close();
                }

            }
            if (results.Any())
                ShowData(results);
            Console.WriteLine(string.Empty);
            Console.WriteLine(string.Format("-----ADO.NET查詢資料數為{0}筆-----", results.Count));

            return results;
        }

        /// <summary>
        /// 查詢資料 By Dapper
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Users> SelectUserByDapper(int[] Ids = null)
        {
            Console.WriteLine(string.Empty);
            Console.WriteLine("----------使用Dapper查詢----------");

            List<Users> results = new List<Users>();
            Users result = new Users();
            using (SqlConnection conn = new SqlConnection(DefaultConn))
            {

                string strSql = @"
select [UserId]
      ,[Email]
      ,[Name]
      ,[IsEnable]
      ,[GuidID]
      ,[Money]
      ,[LastUpdateTime] from Users ";

                if (Ids != null && Ids.Any())
                {
                    strSql += @" Where UserId in @UserIds ";
                }

                strSql += ";";

                //查詢
                results = conn.Query<Users>(strSql, new { UserIds = Ids }).ToList();

                //查詢第一筆 (不可以查無資料)
                //result = conn.QueryFirst<Users>(strSql);

                //查詢第一筆 (可以查無資料)
                //result = conn.QueryFirstOrDefault<Users>(strSql);

                //查詢唯一一筆 (不可以查無資料)
                //result = conn.QuerySingle<Users>(strSql);

                //查詢唯一一筆 (可以查無資料)
                //result = conn.QuerySingleOrDefault<Users>(strSql);

                //results.Add(result);
            }
            if (results.Any())
                ShowData(results);
            Console.WriteLine(string.Empty);
            Console.WriteLine(string.Format("-----Dapper查詢資料數為{0}筆-----", results.Count));

            return results;
        }

        /// <summary>
        /// select use dynamic
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<dynamic> SelectUseDynamic()
        {
            Console.WriteLine(string.Empty);
            Console.WriteLine("----------查詢使用dynamic類別----------");
            List<dynamic> results = new List<dynamic>();

            using (SqlConnection conn = new SqlConnection(DefaultConn))
            {
                string strSql = @"
select [UserId]
      ,[Email]
      ,[Name]
      ,[IsEnable]
      ,[GuidID]
      ,[Money]
      ,[LastUpdateTime] from Users ";

                results = conn.Query<dynamic>(strSql).ToList();

                foreach (var item in results)
                {
                    
                    Console.WriteLine(string.Format("UserId:{0}", item.UserId));
                    Console.WriteLine(string.Format("Email:{0}", item.Email ?? string.Empty));
                }
            }

            Console.WriteLine(string.Empty);
            Console.WriteLine(string.Format("-----查詢資料數為{0}筆-----", results.Count));

            return results;
        }

        /// <summary>
        /// Dapper 新增
        /// </summary>
        /// <param name="models">新增的資料</param>
        /// <returns></returns>
        private static int InsertUsers(IEnumerable<Users> models)
        {
            Console.WriteLine(string.Empty);
            Console.WriteLine("----------新增資料----------");
            int result = 0;

            using (SqlConnection conn = new SqlConnection(DefaultConn))
            {
                string strSql = @"
INSERT INTO [Users]
           ([Email]
           ,[Name]
           ,[IsEnable]
           ,[GuidID]
           ,[Money]
           ,[LastUpdateTime])
     VALUES
           (@Email,
            @Name,
            @IsEnable, 
            @GuidID,
            @Money, 
            @LastUpdateTime) ;
";
                result = conn.Execute(strSql, models);
            }

            Console.WriteLine(string.Empty);
            Console.WriteLine(string.Format("-----新增資料{0}筆-----", result));

            return result;
        }

        /// <summary>
        /// Dapper 修改
        /// </summary>
        /// <param name="models">修改的資料</param>
        /// <returns></returns>
        private static int UpdateUsers(IEnumerable<Users> models)
        {
            Console.WriteLine(string.Empty);
            Console.WriteLine("----------修改資料----------");

            int result = 0;

            using (SqlConnection conn = new SqlConnection(DefaultConn))
            {
                string strSql = @"
     UPDATE [Users]
     SET    Email = @Email,
            Name = @Name,
            IsEnable = @IsEnable, 
            GuidID = @GuidID,
            Money = @Money, 
            LastUpdateTime = @LastUpdateTime
     WHERE  UserId = @UserId ;
";
                result = conn.Execute(strSql, models);
            }

            Console.WriteLine(string.Empty);
            Console.WriteLine(string.Format("-----修改資料{0}筆-----", result));

            return result;
        }

        /// <summary>
        /// Dapper 刪除
        /// </summary>
        /// <param name="models">刪除的資料</param>
        /// <returns></returns>
        private static int DeleteUsers(IEnumerable<Users> models)
        {
            Console.WriteLine(string.Empty);
            Console.WriteLine("----------刪除資料----------");

            int result = 0;

            using (SqlConnection conn = new SqlConnection(DefaultConn))
            {
                string strSql = @"
     DELETE FROM [Users]
     WHERE  UserId = @UserId ;
";
                result = conn.Execute(strSql, models);
            }

            Console.WriteLine(string.Empty);
            Console.WriteLine(string.Format("-----刪除資料{0}筆-----", result));

            return result;
        }

        /// <summary>
        /// 顯示資料
        /// </summary>
        /// <param name="datas"></param>
        private static void ShowData(IEnumerable<Users> datas)
        {
            foreach (var item in datas)
            {
                Console.WriteLine(string.Format(@"UserId:{0},Name:{1},Email:{2},IsEnable:{3},GuidID:{4},Money:{5},LastUpdateTime:{6}",
                    item.UserId, item.Name, item.Email, item.IsEnable, item.GuidID, item.Money, item.LastUpdateTime));
            }
        }

        #endregion

        #region 資料交易

        /// <summary>
        /// Transaction
        /// </summary>
        private static void Transaction()
        {
            Console.WriteLine(string.Empty);
            Console.WriteLine("----------Transaction----------");

            int result = 0;

            using (SqlConnection conn = new SqlConnection(DefaultConn))
            {
                List<Users> update1 = new List<Users>() {
                    new Users(){
                        UserId = 1003,
                        Email = "ccc@gmail.com"
                    }
                };

                List<Users> update2 = new List<Users>(){
                    new Users(){
                        UserId = 1004,
                        Email = "ccc@gmail.com"
                    }
                };

                string strSql = @"
     UPDATE [Users]
     SET    Email = @Email
     WHERE  UserId = @UserId ;
";

                conn.Open();

                //交易
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    int result1 = conn.Execute(strSql, update1, tran);

                    #region 故意出錯測試 update1 資料是否有修改
                    //string str = null;
                    //str.ToString();
                    #endregion

                    int result2 = conn.Execute(strSql, update2, tran);

                    tran.Commit();

                    result += result1;
                    result += result2;
                }
            }

            Console.WriteLine(string.Empty);
            Console.WriteLine(string.Format("-----Transaction異動資料{0}筆-----", result));
        }

        /// <summary>
        /// TransactionScope
        /// </summary>
        private static void TransactionScope()
        {
            Console.WriteLine(string.Empty);
            Console.WriteLine("----------TransactionScope----------");

            int result = 0;
            int result1 = 0;
            int result2 = 0;
          string strSql = @"
     UPDATE [Users]
     SET    Email = @Email
     WHERE  UserId = @UserId ;
";

            using (TransactionScope scope = new TransactionScope())
            {
                using (SqlConnection conn = new SqlConnection(DefaultConn))
                {
                    List<Users> update1 = new List<Users>() {
                    new Users(){
                        UserId = 1003,
                        Email = "xxx@gmail.com"
                    }};

                    List<Users> update2 = new List<Users>(){
                    new Users(){
                        UserId = 1004,
                        Email = "xxx@gmail.com"
                    }};

                    result1 = conn.Execute(strSql, update1);

                    #region 故意出錯測試 update1 資料是否有修改
                    //string str = null;
                    //str.ToString();
                    #endregion

                    result2 = conn.Execute(strSql, update2);
                }

                scope.Complete();
                result += result1;
                result += result2;
            }

            Console.WriteLine(string.Empty);
            Console.WriteLine(string.Format("-----TransactionScope異動資料{0}筆-----", result));
        }

        #endregion
    }

    class Users
    {
        public int UserId { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public bool IsEnable { get; set; }

        public Nullable<Guid> GuidID { get; set; }

        public Nullable<decimal> Money { get; set; }

        public Nullable<DateTime> LastUpdateTime { get; set; }
    }
}
