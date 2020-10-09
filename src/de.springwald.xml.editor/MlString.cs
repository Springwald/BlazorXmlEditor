using System.Globalization;
using System.Reflection;
using System.Resources;

namespace MultiLang
{
    internal class ml
    {
        private static string RootNamespace = "de.springwald.xml"; //MLHIDE
        private static ResourceManager ResMgr;

        static ml()
        {
            ResMgr = new ResourceManager(RootNamespace + ".MultiLang", Assembly.GetExecutingAssembly()); //MLHIDE
        }

        public static void ml_UseCulture(CultureInfo ci)
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
        }

        public static string ml_string(int StringID, string Text)
        {
            return ml_resource(StringID);
        }

        public static string ml_resource(int StringID)
        {
            return ResMgr.GetString("_" + StringID.ToString());
        }
        public static string[] SupportedCultures = { "de" }; //MLHIDE
    }
}
