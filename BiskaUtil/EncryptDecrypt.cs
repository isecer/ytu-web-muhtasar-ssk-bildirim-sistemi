using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace BiskaUtil
{
    public static class EncryptDecrypt
    {
        private static byte[] key = { 33, 35, 99, 41, 24, 26, 201, 45};
        private static byte[] IV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xab, 0xcd, 0xef };


        public static string Decrypt(this string stringToDecrypt, string sEncryptionKey)
        {
            byte[] inputByteArray = new byte[stringToDecrypt.Length + 1];            
            try
            {
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                if(string.IsNullOrWhiteSpace(sEncryptionKey)==false)
                  key = System.Text.Encoding.UTF8.GetBytes(sEncryptionKey);
                inputByteArray = Convert.FromBase64String(stringToDecrypt);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms,
                  des.CreateDecryptor(key, IV), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                System.Text.Encoding encoding = System.Text.Encoding.UTF8;
                return encoding.GetString(ms.ToArray());
            }
            catch 
            {
                return "";
            }            
        }
        public static string Decrypt(this string stringToDecrypt)
        {
            byte[] inputByteArray = new byte[stringToDecrypt.Length + 1];
            try
            {
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();                
                inputByteArray = Convert.FromBase64String(stringToDecrypt);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms,
                  des.CreateDecryptor(key, IV), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                System.Text.Encoding encoding = System.Text.Encoding.UTF8;
                return encoding.GetString(ms.ToArray());
            }
            catch 
            {
                return "";
            }
        }

        public static string Encrypt(this string stringToEncrypt, string sEncryptionKey)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sEncryptionKey) == false)
                   key = System.Text.Encoding.UTF8.GetBytes(sEncryptionKey);
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                byte[] inputByteArray = Encoding.UTF8.GetBytes(stringToEncrypt);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms,
                  des.CreateEncryptor(key, IV), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        public static string Encrypt(this string stringToEncrypt)
        {
            try
            {                
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                byte[] inputByteArray = Encoding.UTF8.GetBytes(stringToEncrypt);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms,
                  des.CreateEncryptor(key, IV), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }



        public static string ComputeHash(this string sifre, string tuz = null)
        {
            SHA256Managed sha = new SHA256Managed();
            //var tuz = "M@V0R£";
            //sifre = sifre+ tuz;
            if (tuz != null)
                sifre += tuz;
            byte[] sifreBytes = Encoding.UTF8.GetBytes(sifre);
            byte[] ozetBytes = sha.ComputeHash(sifreBytes);
            string hesaplananOzetSifre = Convert.ToBase64String(ozetBytes);
            return hesaplananOzetSifre;
        }
    }
}