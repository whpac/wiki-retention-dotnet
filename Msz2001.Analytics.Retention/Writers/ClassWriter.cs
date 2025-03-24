namespace Msz2001.Analytics.Retention.Writers
{
    internal class ClassWriter
    {
        public static void Write(string fileName, string[] classes, Dictionary<string, Dictionary<string, uint>> monthlyCounts)
        {
            using var classFileStream = File.Open(fileName, FileMode.Create, FileAccess.Write);
            using var classWriter = new StreamWriter(classFileStream);

            classWriter.Write("Month");
            foreach (var className in classes)
            {
                classWriter.Write("\t" + className);
            }
            classWriter.WriteLine();
            foreach (var (month, userClasses) in monthlyCounts.OrderBy(e => e.Key))
            {
                classWriter.Write(month);
                foreach (var className in classes)
                {
                    classWriter.Write("\t" + userClasses[className]);
                }
                classWriter.WriteLine();
            }
        }
    }
}
