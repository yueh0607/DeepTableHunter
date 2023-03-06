
using System.IO;
using System.Net;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading.Tasks;
using OpenQA.Selenium.Support.UI;
using Newtonsoft.Json;
namespace TH
{

    public class Config
    {
        public int mode = 0;

        public string url = "https://www.chaojibiaoge.com/Form/webform_records/sharekey/7x9ppx8y/viewrecords/allRecords";
        public string[] items = { "计算机科学与技术", "软件工程", "空间信息与数字技术", "数据科学与大数据技术" };
        public string[] orders = { "第一志愿", "第二志愿" ,"第三志愿","第四志愿"};
        public float delayTime = 5f;
        public string total = "计科:100    软工:80    空间:79    大数据:40";

    }

    public class Program
    {
        public static string text = null;
        public static Config config;
        public static void Main()
        {
            WriteJsonDefault();
            string json = GetAll("config.json");
            config = JsonConvert.DeserializeObject<Config>(json);
            if (config == null) throw new Exception("Config Exception");
            else Console.WriteLine("配置文件加载成功!");

            Console.WriteLine("开启浏览器行为模拟-状态日志：");


            if (config.mode == 0)
            {
                DownloadFromChormeAsync(config.url);
                Console.WriteLine("网络信息加载中...请等待约 " + config.delayTime + " s");
                Console.Write("如果超时，则重启或检查程序的异常: ");
                PrintTime(config.delayTime);
                while (text == null) ;
            }
            else
            {
                //本地HTML方式
                Console.Write("HTMLPATH:");
                text = GetAll(Console.ReadLine());
            }


            int allCount = 0;
            bool sign = true;
            Console.WriteLine("---------------------------------------------------------------");
            foreach (var order in config.orders)
            {
                foreach (var tar in config.items)
                {
                    int c = GetCount(text, tar, order);
                    if (sign) allCount += c;
                    string name = tar.Substring(0, 2) + "-" + order + ": " + c + " 人";
                    Console.WriteLine(name);
                }
                sign = false;
                Console.WriteLine("---------------------------------------------------------------");
            }

            Console.WriteLine("计科:100    软工:80    空间:79    大数据:40");
            Console.WriteLine($"\n共计: {allCount} 人已填写表单");


            Console.ReadKey();
        }


        public static void WriteJsonDefault()
        {
            if (File.Exists("config.json")) return;

            string x = JsonConvert.SerializeObject(new Config(), Formatting.Indented);
            StreamWriter sr = new StreamWriter("config.json");
            sr.Write(x);
            sr.Dispose();
        }
        public static int GetCount(string text, string target, string order)
        {
            string vk = "<div class=\"fieldname\" title=\"第一志愿\">第一志愿:</div><div class=\"fieldvalue\">计算机科学与技术</div>";
            vk = vk.Replace("第一志愿", order);
            vk = vk.Replace("计算机科学与技术", target);
            string key = "#*-*---*--*#";


            var xs = text.Replace(vk, key).Split(key);
            return xs.Length - 1;
        }

        private static string GetAll(string path)
        {
            using (var x = new StreamReader(path))
            {
                return x.ReadToEnd();
            }
        }

        private static async void DownloadFromChormeAsync(string url)
        {
            text = await GetHTMLByChorme(url);
        }

        private static async Task<string> GetHTMLByChorme(string url)
        {

            ChromeOptions options = new ChromeOptions();
            options.AddArgument("headless");
            using (var driver = new ChromeDriver(options))
            {
                driver.Navigate().GoToUrl(url);

                await Task.Delay((int)(config.delayTime) * 1000);
                //wait.Until(driver => driver.FindElement(By.XPath("//div[@class='dynamic-content']"))); // 等待动态内容的元素加载完成
                string retString = await Task.Run(() => driver.PageSource);
                //Console.WriteLine(retString);
                return retString;
            }
        }

        private static async void PrintTime(float time)
        {
            for(float i=time;i>0;i--)
            {
                await Task.Delay(1000);
                if (text != null) return;
                Console.Write(" "+i);
            }
            
        }




    }
}