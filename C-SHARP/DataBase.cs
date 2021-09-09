using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using th2.bean;
using th2.config;

namespace th2.other
{
    static class DataBase//静态
    {
        static string _dBConnStr;//数据库连接语句本体
        static MySqlConnection _mySqlConnection;//连接

        //将连接语句更新写入app.config里的connectionStrings标签
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

        //从配置文件返回连接语句
        public static bool GetConnectionStringsConfig()
        {
            try
            {
                _dBConnStr = DBconfig.GetConnectionStringsConfig("MySqlConn");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //用语句得到连接
        public static MySqlConnection GetMySqlConnection()
        {
            GetConnectionStringsConfig();
            _mySqlConnection = new MySqlConnection(_dBConnStr);
            return _mySqlConnection;
        }

        public static bool DatabaseOpen()
        {
            GetMySqlConnection();
            try
            {
                _mySqlConnection.Open();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                _mySqlConnection.Close();
            }
        }

        public static MySqlDataReader GetSqlDataRead0(string sql)
        {
            try
            {
                _mySqlConnection.Open();
                var sqlCommand = new MySqlCommand(sql, _mySqlConnection);
                var sqlRead0 = sqlCommand.ExecuteReader();
                return sqlRead0;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static MySqlDataReader GetSqlDataRead1(string sql)
        {
            MySqlDataReader sqlRead0 = GetSqlDataRead0(sql);
            sqlRead0.Read();
            MySqlDataReader sqlRead1 = sqlRead0;
            return sqlRead1;
        }


        public static bool IsUserLogin(string user, string userPhone, string password)//登录
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
                _mySqlConnection.Close();
            }
        }


        public static bool UserLogOn(string user, string userName, string password, string userPhone, string power)//注册新账户
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
                _mySqlConnection.Close();
            }
        }


        public static void SelectAllUserInfo()//查询全部用户信息//
        //由于在GetSqlRead里Read过一次,当在while循环中第一次Read时,实为第二次Read,固没有查询第列数据,
        //可把第一列数据当作测试数据
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
            _mySqlConnection.Close();
        }

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
            _mySqlConnection.Close();
            return allUserList;
        }


        public static void SelectAllLogInfo() //查询全部日志信息
        {
            LogInfo logInfo = new LogInfo();
            string sqlSelectAllLogInfo = "select * from log_info";
            MySqlDataReader allLogInfoRead = GetSqlDataRead0(sqlSelectAllLogInfo);
            while (allLogInfoRead.Read())
            {
                logInfo.setLoginInfoList(allLogInfoRead.GetInt32(0),allLogInfoRead.GetString(2),allLogInfoRead.GetString(3),allLogInfoRead.GetString(4));
                logInfo.addAllLoginInfo();
            }
            _mySqlConnection.Close();
        }

        public static bool UpdatePassword(Int32 id, string password)//修改密码
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
                _mySqlConnection.Close();
            }
        }
        public static void RecordLog() //日志
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
                _mySqlConnection.Close();
            }
        }

        public static int VerifyUser(string fgUserText, string fgNameText, string fgPhoneText)//验证账号
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
                _mySqlConnection.Close();
            }
        }

        public static bool DeleteUserByUser(string user)//删除账号
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
                _mySqlConnection.Close();
            }
        }

        public static bool UpdateUserByUser(string userName, string password, string userPhone, string power, string user) //编辑账号
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
                _mySqlConnection.Close();
            }

        }
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
                _mySqlConnection.Close();
            }
        }
    }
}