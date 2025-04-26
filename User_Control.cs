using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBotWin
{
    public static class User_Control
    {
        public static string version = "USERCONTROL VER 1.5";
        public enum Permissions
        {
            GUEST = 0,
            USER = 1,
            ADMIN = 2
        }

        public static Dictionary<string, Permissions> user_permssions = new Dictionary<string, Permissions>();

        public static void SavePermission()
        {
            using (FileStream fs = new FileStream("Permissions.csv", FileMode.Create))
            {
                string s = "";
                foreach (var x in user_permssions)
                {
                    s += $"{x.Key},{x.Value}\n";
                }
                s.Remove(s.Length - 1 - "\n".Length,"\n".Length);
                fs.Write(Encoding.UTF8.GetBytes(s));
                fs.Close();
            }
        }

        public static void LoadPermission() 
        {
            if (!File.Exists("Permissions.csv"))
            {
                return;
            }
            using (FileStream fs = new FileStream("Permissions.csv", FileMode.Open))
            {
                string s = new StreamReader(fs).ReadToEnd();
                if (string.IsNullOrEmpty(s))
                {
                    return;
                }
                foreach (var x in s.Split("\n"))
                {
                    if (string.IsNullOrEmpty(x))
                    {
                        break;
                    }
                    string[] t = x.Split(",");
                    user_permssions.Add(t[0], (Permissions)Enum.Parse(typeof(Permissions), t[1]));
                }
                fs.Close();
            }
        }

        public static bool VerifyPermission(string usr,Permissions p)
        {
            if (user_permssions.Keys.Contains(usr))
            {
                return (user_permssions[usr] >= p);
            }
            else
            {
                user_permssions.Add(usr, Permissions.USER);
                SavePermission();
                return (int)p <= 1;
            }
        }
        
    }
}
