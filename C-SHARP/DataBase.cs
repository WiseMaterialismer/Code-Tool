using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using th2.bean;
using th2.config;

namespace th2.other
{
    static class DataBase
    {
        static string dBConnStr;//数据库连接语句本体
        static MySqlConnection mySqlConnection;//连接

        //将连接语句更新写入app.config里的connectionStrings标签//
        public static bool UpdateConnectionStringsConfig(String newName, String newConString, String newProviderName)
        {
            try
            {
                DBconfig.UpdateConnectionStringsConfig(newName, newConString, newProviderName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //从配置文件返回连接语句//
        public static bool GetConnectionStringsConfig()
        {
            try
            {
                dBConnStr = DBconfig.GetConnectionStringsConfig("MySqlConn");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //用语句得到连接//
        public static MySqlConnection GetMySqlConnection()
        {
            GetConnectionStringsConfig();
            mySqlConnection = new MySqlConnection(dBConnStr);
            return mySqlConnection;
        }

        //开启数据库//
        public static bool DatabaseOpen()
        {
            GetMySqlConnection();
            try
            {
                mySqlConnection.Open();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                mySqlConnection.Close();
            }
        }

        //得到没有被Read过的Read数据//
        public static MySqlDataReader GetSqlDataRead0(string sql)
        {
            try
            {
                mySqlConnection.Open();
                var sqlCommand = new MySqlCommand(sql, mySqlConnection);
                var sqlRead0 = sqlCommand.ExecuteReader();
                return sqlRead0;
            }
            catch (Exception)
            {
                return null;
            }
        }
        //Read过一次的Read数据//
        public static MySqlDataReader GetSqlDataRead1(string sql)
        {
            MySqlDataReader sqlRead0 = GetSqlDataRead0(sql);
            sqlRead0.Read();
            MySqlDataReader sqlRead1 = sqlRead0;
            return sqlRead1;
        }

        //登录//
        public static bool IsUserLogin(string user, string userPhone, string password)
        {
            String sqlLogin = string.Format("select * from user where (user = '{0}'and password = '{1}')or (user_phone = '{2}'and password = '{1}')", user, password, userPhone);//SQL语句查找账号密码
            try
            {
                MySqlDataReader loginRead = GetSqlDataRead0(sqlLogin);
                bool isUserLogin = loginRead.Read();
                UserInfo.Id = loginRead.GetInt32(0);
                UserInfo.User = loginRead.GetString(1);
                UserInfo.UserName = loginRead.GetString(2);
                UserInfo.Password = loginRead.GetString(3);
                UserInfo.UserPhone = loginRead.GetString(4);
                UserInfo.Power = loginRead.GetString(5);
                return isUserLogin;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                mySqlConnection.Close();
            }
        }

        //注册新账户//
        public static bool UserLogOn(string user, string userName, string password, string userPhone, string power)
        {
            string sqlLogon = string.Format(@"INSERT INTO user(user,user_name,password,user_phone,power) VALUES('{0}','{1}','{2}','{3}','{4}')", user, userName, password, userPhone, power);
            try
            {
                GetSqlDataRead0(sqlLogon);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                mySqlConnection.Close();
            }
        }

        //查询全部用户信息//
        public static void SelectAllUserInfo()
        {
            string sqlSelectAllUserInfo = "select * from user";//查询全部用户信息的sql语句"从user表查询全部数据"
            MySqlDataReader allUserInfoRead = GetSqlDataRead0(sqlSelectAllUserInfo);//将sql语句调入到GetSqlDataRead中得到DataReader数据
            while (allUserInfoRead.Read())//如果为true就一直Read下去
            {
                UserInfo.setUserInfoList( allUserInfoRead.GetString(1),
                    allUserInfoRead.GetString(2),allUserInfoRead.GetString(4),
                    allUserInfoRead.GetString(5));
                UserInfo.addAllInfoList();
                //一个用户全部的信息被添加完毕,将这个用户信息当成一个信息添加到userInfoList集合中
            }
            mySqlConnection.Close();
        }

        //查询所有的User//
        public static List<string> SelectAllUser()
        {
            List<string> allUserList = new List<string>();
            string sqlSelectAllUser = @"select user from user";
            MySqlDataReader allUserReader = GetSqlDataRead0(sqlSelectAllUser);
            while (allUserReader.Read())
            {
                string userListInfo = allUserReader.GetString(0);
                allUserList.Add(userListInfo);
            }
            mySqlConnection.Close();
            return allUserList;
        }

        //查询全部日志信息//
        public static void SelectAllLogInfo() 
        {
            LogInfo logInfo = new LogInfo();
            string sqlSelectAllLogInfo = "select * from log_info";
            MySqlDataReader allLogInfoRead = GetSqlDataRead0(sqlSelectAllLogInfo);
            while (allLogInfoRead.Read())
            {
                logInfo.setLoginInfoList(allLogInfoRead.GetInt32(0),allLogInfoRead.GetString(2),allLogInfoRead.GetString(3),allLogInfoRead.GetString(4));
                logInfo.addAllLoginInfo();
            }
            mySqlConnection.Close();
        }

        //修改密码//
        public static bool UpdatePassword(Int32 id, string password)
        {
            string sqlAlterPassword = string.Format("update user set password = '{0}' where id = {1}", password, id);
            try
            {
                GetSqlDataRead0(sqlAlterPassword);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                mySqlConnection.Close();
            }
        }

        //通过User,UserName,userPhone验证账号//
        public static int VerifyUser(string fgUserText, string fgNameText, string fgPhoneText)
        {
            string sqlVerifyUser =
                string.Format(@"select id from user where ( user = '{0}' and user_name = '{1}' ) and user_phone = '{2}'",
                    fgUserText, fgNameText, fgPhoneText);
            try
            {
                MySqlDataReader verifyUserRead = GetSqlDataRead1(sqlVerifyUser);
                return verifyUserRead.GetInt32(0);
            }
            catch (Exception)
            {
                return 0;
            }
            finally
            {
                mySqlConnection.Close();
            }
        }

        //根据user删除账号//
        public static bool DeleteUserByUser(string user)
        {
            string sqlDeleteById = string.Format(@"delete from user where user = '{0}'", user);

            try
            {
                MySqlDataReader deleteByIdRead = GetSqlDataRead0(sqlDeleteById);
                deleteByIdRead.Read();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                mySqlConnection.Close();
            }
        }

        //编辑账号//
        public static bool UpdateUserByUser(string userName, string password, string userPhone, string power, string user) 
        {
            string sqlUpdateByUser = string.Format(@"update user set user_name = '{0}', password = '{1}',user_phone = '{2}',power = '{3}' where user = '{4}'",userName,password,userPhone,power,user);
            try
            {
                MySqlDataReader updateUserByUserReader = GetSqlDataRead0(sqlUpdateByUser);
                updateUserByUserReader.Read();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                mySqlConnection.Close();
            }

        }


        //记录日志//
        public static void RecordLog()
        {
            string sqlRecordLog =
                string.Format(@"INSERT INTO log_info (login_id, login_user, login_name) VALUES ({0}, '{1}', '{2}');",
                    UserInfo.Id, UserInfo.User, UserInfo.UserName);
            try
            {

                GetSqlDataRead0(sqlRecordLog);
            }
            catch (Exception)
            {
                string sqlRecordLogWrong =
                    string.Format(
                        @"INSERT INTO log_info (login_id, login_user, login_name) VALUES ({0}, '{1}', '{1}');", 99999,
                        "error");
                GetSqlDataRead0(sqlRecordLogWrong);
            }
            finally
            {
                mySqlConnection.Close();
            }
        }

        //根据id删除日志//
        public static bool DeleteLogById(string id)
        {
            string sqlDeleteById = string.Format(@"delete from log_info where id = '{0}'", id);

            try
            {
                MySqlDataReader deleteByIdRead = GetSqlDataRead0(sqlDeleteById);
                deleteByIdRead.Read();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                mySqlConnection.Close();
            }
        }

        //记录登录信息//
        public static void RecordAstrict()
        {
            string sqlRecordLog =
                string.Format(@"INSERT INTO login_astrict (user) VALUES ({0});",
                    UserInfo.User);
            try
            {

                GetSqlDataRead0(sqlRecordLog);
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                mySqlConnection.Close();
            }
        }

        //检查现存的登录信息个数//
        public static int selectAstrictNum()
        {
            string sqlSelectAstrictNum = @"select * from login_astrict";
            int i = 0;
            try
            {
                MySqlDataReader mySqlDataReader = GetSqlDataRead0(sqlSelectAstrictNum);
                while (mySqlDataReader.Read())
                {
                    i++;
                }
                return i;
            }
            catch (Exception )
            {
                return 0;
            }
            finally
            {
                mySqlConnection.Close();
            }
        }

        //根据id删除登录信息//
        public static bool DeleteAstrictById()
        {
            string sqlDeleteById = string.Format(@"delete from login_astrict where id = '{0}'", UserInfo.Id);

            try
            {
                MySqlDataReader deleteByIdRead = GetSqlDataRead0(sqlDeleteById);
                deleteByIdRead.Read();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                mySqlConnection.Close();
            }
        }

    }
}