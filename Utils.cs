using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExtensionMethods;
//using UnityEngine;

namespace ExtensionMethods
{
    public static class MyExtensionMethods
    {
        public static string time_ago(this DateTime date)
        {
            const int SECOND = 1;
            const int MINUTE = 60 * SECOND;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            const int MONTH = 30 * DAY;

            var ts = new TimeSpan(DateTime.UtcNow.Ticks - date.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 1 * MINUTE)
                return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";

            if (delta < 2 * MINUTE)
                return "a minute ago";

            if (delta < 45 * MINUTE)
                return ts.Minutes + " minutes ago";

            if (delta < 90 * MINUTE)
                return "an hour ago";

            if (delta < 24 * HOUR)
                return ts.Hours + " hours ago";

            if (delta < 48 * HOUR)
                return "yesterday";

            if (delta < 30 * DAY)
                return ts.Days + " days ago";
            else
            {
                return "on " + date.ToString("yyyy/MM/dd");
            }
        }    
    }
}

namespace KatLib
{
    public class Checksum
    {

        public static bool compare(string str1, string str2){
            return digest(str1) == digest(str2);           
        }

        public static string digest(string text){
            if(String.IsNullOrEmpty(text))
                return String.Empty;

            using(var sha = new System.Security.Cryptography.SHA256Managed()){
                byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }
    }


    public class Paths
    {
        //takes any number of strings and returns them joined together with Linux specific path divider, ie:
        //Paths.joined("follow", "the", "yellow", "brick", "road") -> "follow/the/yellow/brick/road 
        static public string joined(params string[] paths){
            return String.Join("/", paths).Replace("\\", "/");
        }

        //takes any number of strings and returns them joined together with OS specific path divider, ie:
        //Paths.joined("follow", "the", "yellow", "brick", "road") -> "follow/the/yellow/brick/road or follow\the\yellow\brick\road (I mean, what kinda os uses \ anyway, madness).
        static public string os_joined(params string[] paths){
            return String.Join(Path.DirectorySeparatorChar.ToString(), paths);
        }

        static public string os_safe(string path_string){
            path_string = path_string.Replace('\\', '/');
            return path_string.Replace('/', Path.DirectorySeparatorChar);
        }

    }


    //The most redneck implementation of a JSON serializer ever!
    //it can take a Dictionary<string, object> and the objects in the dictionary can be a string, a numeric, or a nested Dictionary<string, object>
    //it can also take an optional second argument which is either a bool or an int.  If given as (bool)true then it will generate JSON with spacing and indentation
    //(the same arg is also used to pass on the level of indentation when called recusively)
    //handle with caution, if swallowed seek medical attention.
    public class JSONX
    {

        public static string toJSON(Dictionary<string, string> data, params object[] opts){
            Dictionary<string, object> n_data = new Dictionary<string, object>();
            foreach(KeyValuePair<string, string> pair in data){
                n_data.Add(pair.Key, (object)pair.Value);
            }
            return toJSON(n_data, opts);
        }

        public static string toJSON(Dictionary<string, object> data, params object[] opts){
            int indent = 0;
            bool do_indent = false;
            if(opts.Length == 1){
                if(opts[0] is int){
                    indent = (int)opts[0];
                    do_indent = true;
                } else{
                    do_indent = (bool)opts[0];
                }
            }
            indent++;
            object arg;
            if(do_indent){
                arg = (int)indent;
            } else{
                arg = (bool)do_indent;
            }  //yes it's a if else block on one line, deal with it, C# refused to let me use a ternary with mixed types, fussy lang.

            List<string> objects = new List<string>();

            foreach(KeyValuePair<string, object> entry in data){
                if(entry.Value is Dictionary<string, object>){
                    var t = (Dictionary<string, object>)data[entry.Key];
                    objects.Add(String.Format("\"{0}\":{1}", entry.Key, JSONX.toJSON(t, arg)));
                } else if(entry.Value is String){
                    objects.Add(String.Format("\"{0}\":\"{1}\"", entry.Key, entry.Value));
                }else if(entry.Value is List<string>){
                    List<string> list = (List<string>)entry.Value;
                    objects.Add(String.Format("\"{0}\":[\"{1}\"]", entry.Key, String.Join("\",\"", list.ToArray()) ));
                } else{ 
                    //mutha of asumptions: if it's not a string or dict then it's a numeric, cos seriously C#, you don't have a numeric class? You want me to test for each 
                    //numerical type individualy? yeah....I'm to lazy for that...oh Ruby...I miss you so.
                    try{
                        objects.Add(String.Format("\"{0}\":{1}", entry.Key, entry.Value));
                    } catch{
                        objects.Add(String.Format("\"{0}\":\"{1}\"", entry.Key, entry.Value.ToString()));
                    }                   
                }
            }

            //Poor man's String.join becuase I couldn't get the line below to function in unity
            //string json_string = "{" + String.Join (",", objects) + "}";
            //also adds in spaces and new lines if do_indent is true.
            string json_string = "{";
            foreach(string obj in objects){ 
                if(do_indent){
                    json_string = json_string + "\n";
                    for(int i = 0; i < indent; i++){
                        json_string = json_string + "    ";
                    }
                }
                json_string = json_string + obj;
                if(obj != (objects.Count==0 ? null : objects[objects.Count-1])){
                    json_string = json_string + ",";
                }
            }
            if(do_indent){
                json_string = json_string + "\n";
                for(int i = 0; i < indent - 1; i++){
                    json_string = json_string + "    ";
                }
            }
            json_string = json_string + "}";

            return json_string; //hey look, some JSON!
        }

    }

}

