﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using Capstone.Models;
using Capstone.Security;
using Capstone.Security.Models;
using Microsoft.AspNetCore.Mvc;

namespace Capstone.DAO
{
    public class UserSqlDao : IUserDao
    {
        private readonly string connectionString;

        public UserSqlDao(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        public User GetUser(string username)
        {
            User returnUser = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    SqlCommand cmd = new SqlCommand("SELECT users.user_id, username, password_hash, salt, user_role, email, street_address, city, state_abbreviation, zip_code, is_premium, collection.is_public " +
                        "FROM users JOIN collection ON users.user_id = collection.user_id WHERE username = @username", conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        returnUser = GetUserFromReader(reader);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return returnUser;
        }
        public List<string> GetUsernamesBySearch(string searchString)
        {
            List<string> usernames = new List<string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT username " +
                        "FROM users WHERE username LIKE @username;", conn);
                    cmd.Parameters.AddWithValue("@username","%" + searchString + "%");
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string username = Convert.ToString(reader["username"]);
                        usernames.Add(username);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return usernames;
        }

        public List<string> GetUsernamesOfMyActiveTrades(string username)
        {
            List<string> usernames = new List<string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT " +
                                                        "username FROM users " +
                                                    "WHERE " +
                                                        "(users.user_id IN " +
                                                        "(SELECT user_id_from FROM trade WHERE user_id_to = " +
                                                        "(SELECT user_id FROM users WHERE username = @username) " +
                                                        "AND trade.status = 'pending') " +
                                                    "OR " +
                                                        "users.user_id IN " +
                                                        "(SELECT user_id_to FROM trade WHERE user_id_from = " +
                                                        "(SELECT user_id FROM users WHERE username = @username) " +
                                                        "AND trade.status = 'pending'))", conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string trader = Convert.ToString(reader["username"]);
                        usernames.Add(trader);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return usernames;
        }

        public User AddUser(string username, string password, string role, string email, string streetAddress, string city, string stateAbbreviation, string zipCode)
        {
            IPasswordHasher passwordHasher = new PasswordHasher();
            PasswordHash hash = passwordHasher.ComputeHash(password);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("INSERT INTO users (username, password_hash, salt, user_role, email, street_address, city, state_abbreviation, zip_code) " +
                        "VALUES (@username, @password_hash, @salt, @user_role, @email, @street_address, @city, @state_abbreviation, @zip_code)", conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password_hash", hash.Password);
                    cmd.Parameters.AddWithValue("@salt", hash.Salt);
                    cmd.Parameters.AddWithValue("@user_role", role);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@street_address", streetAddress);
                    cmd.Parameters.AddWithValue("@city", city);
                    cmd.Parameters.AddWithValue("@state_abbreviation", stateAbbreviation);
                    cmd.Parameters.AddWithValue("@zip_code", zipCode);
                    cmd.ExecuteNonQuery();

                    SqlCommand sqlCommand = new SqlCommand("INSERT INTO collection (user_id) " +
                                                           "VALUES ((SELECT user_id FROM users WHERE username = @username));", conn);
                    sqlCommand.Parameters.AddWithValue("@username", username);
                    sqlCommand.ExecuteNonQuery();
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return GetUser(username);
        }

        public List<PublicCollectionUser> GetPublicUsers()
        {
            List<PublicCollectionUser> publicUsers = new List<PublicCollectionUser>();
            try
            {

                using(SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT username FROM collection " +
                        "JOIN users ON collection.user_id = users.user_id " +
                        "WHERE is_public = 1;", conn);

                    SqlDataReader reader = cmd.ExecuteReader();

                    while(reader.Read())
                    {
                        PublicCollectionUser user = new PublicCollectionUser();
                        user.Username = Convert.ToString(reader["username"]);
                        publicUsers.Add(user);
                    }
                }
            }
            catch(SqlException)
            {
                throw;
            }
            return publicUsers;
        }

        public void ToggleCollectionPrivacy(string userName)
        {
            //int numberOfRowsUpdated = 0;
            try
            {

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("UPDATE collection SET collection.is_public = 1 - collection.is_public " +
                        "FROM collection " +
                        "JOIN users ON users.user_id = collection.user_id " +
                        "WHERE users.username = @username;", conn);
                    cmd.Parameters.AddWithValue("@username", userName);

                    cmd.ExecuteNonQuery();


                }
            }
            catch (SqlException)
            {
                throw;
            }
            //return numberOfRowsUpdated;
        }
        public void ChangeUsersToPremium(string userName)
        {
            //int numberOfRowsUpdated = 0;
            try
            {

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("UPDATE users SET is_premium = 1 WHERE username = @username;", conn);
                    cmd.Parameters.AddWithValue("@username", userName);

                    cmd.ExecuteNonQuery();
                    
                  
                }
            }
            catch (SqlException)
            {
                throw;
            }
            //return numberOfRowsUpdated;
        }

       

        private User GetUserFromReader(SqlDataReader reader)
        {
            User u = new User()
            {
                UserId = Convert.ToInt32(reader["user_id"]),
                Username = Convert.ToString(reader["username"]),
                PasswordHash = Convert.ToString(reader["password_hash"]),
                Salt = Convert.ToString(reader["salt"]),
                Role = Convert.ToString(reader["user_role"]),
                Email = Convert.ToString(reader["email"]),
                StreetAddress = Convert.ToString(reader["street_address"]),
                City = Convert.ToString(reader["city"]),
                StateAbbreviation = Convert.ToString(reader["state_abbreviation"]),
                ZipCode = Convert.ToInt32(reader["zip_code"]),
                isPremium = Convert.ToBoolean(reader["is_premium"]),
                IsPublic = Convert.ToBoolean(reader["is_public"])
            };

            return u;
        }
    }
}
