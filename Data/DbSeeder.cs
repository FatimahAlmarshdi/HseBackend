using HseBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HseBackend.Data
{
    public static class DbSeeder
    {
    public static void SeedQuestions(AppDbContext context)
        {
            if (context.InspectionQuestions.Any())
            {
                return; // Already seeded
            }

            var questions = new List<InspectionQuestion>
            {
                Create(4, "المنطقة نظيفة ومرتبة والأرضيات خالية من العوائق ومن مخاطر الانزلاق", "Choice", "نعم,لا"),
                Create(5, "هل بيئه العمل مناسبة ( الاضاءه , الحراره , التهويه، الضوضاء...إلخ)", "Choice", "نعم,لا"),
                Create(6, "المكان خالي من تراكم المواد أو انبعاث الأغبرة أو تسريب المواد الضارة بالبيئة", "Choice", "نعم,لا,لا ينطبق"),
                Create(7, "جميع الحاويات الكيميائية والزيوت والوقود مخزنة بشكل جيد، ومزودة بأحواض لمنع التسرب وصحيفة السلامة (ام اس دي اس) متوفرة في الموقع", "Choice", "نعم,لا,لا ينطبق"),
                Create(8, "السلالم والأدوار العلوية أو منصات العمل والسقالات خالية من العيوب أو التشققات (مثل البسطات التالفة أو الفتحات الأرضية)، ويتوفر فيها السياج الجانبي للحماية من السقوط", "Choice", "نعم,لا,لا ينطبق"),
                Create(9, "مسالك الإخلاء والسلالم خالية من العوائق ومخارج الطوارئ بحالة جيدة", "Choice", "نعم,لا,لا ينطبق"),
                Create(10, "حواجز وأغطية الحمايه للمعدات متوفرة وتمنع الوصول إلى الأجزاء المتحركة", "Choice", "نعم,لا,لا ينطبق"),
                Create(11, "الموظفين ملتزمين بارتداء معدات السلامة الشخصية المناسبة لطبيعة الموقع ومخاطر العمل", "Choice", "نعم,لا,لا ينطبق"),
                Create(12, "في مواقع تواجد الإشعاع (جميع الحمايات متوفرة ولا توجد قراءة تسريب إشعاعي- القراءة تؤخذ عن طريق كاشف الاشعاع)", "Choice", "نعم,لا,لا ينطبق"),
                Create(13, "المعدات والتوصيلات واللوحات والمقابس الكهربائية بحالة جيدة", "Choice", "نعم,لا"),
                Create(14, "مفاتيح الإيقاف الطارئ للمعدات موجودة وتعمل", "Choice", "نعم,لا,لا ينطبق"),
                Create(15, "معدات الطوارئ والإطفاء والإنذار ومراوش العين وغيرها سهلة الوصول ولا توجد أمامها عوائق", "Choice", "نعم,لا"),
                Create(16, "صناديق الإسعافات الأولية متوفرة ويسهل الوصول لها", "Choice", "نعم,لا,لا ينطبق"),
                Create(17, "اللوحات والعلامات الإرشادية للسلامة والبيئة متوفرة", "Choice", "نعم,لا"),
                Create(18, "أنظمة الإطفاء والإنذار بحالة جيدة (ضغوط طفايات الحريق والمرشات ولوحات الإنذار وصناديق الإطفاء وغيرها بحالة سليمة)", "Choice", "نعم,لا,لا ينطبق"),
                Create(19, "خطة الاخلاء متوفرة في المكان ومثبتة في مكان واضح", "Choice", "نعم,لا,لا ينطبق، ولكن مبنى خارجي سهل الإخلاء"),
                Create(20, "هل يوجد أعمال صيانة؟", "Choice", "نعم، أجب على الأسئلة المتبقية,لا، لا حاجة لإجابة بقية الأسئلة"),
                Create(21, "هل توجد تصاريح العمل اللازمة وعزل الطاقة إن تطلب ذلك", "Choice", "نعم,لا,لا ينطبق"),
                Create(22, "هل المعدات اليدوية والأدوات المستعملة في حالة سليمة", "Choice", "نعم,لا"),
                Create(23, "هل موقع العمل منظم", "Choice", "نعم,لا"),
                Create(24, "في حال توقف أعمال الصيانة لاستئنافها لاحقاً، هل تم إغلاق منطقة العمل بأشرطة تحذيرية أو حواجز", "Choice", "نعم,لا,لا ينطبق"),
                Create(25, "هل يتم تطبيق إجراءات وتعليمات السلامة والبيئة حسب طبيعة العمل ( أعمال باردة، ساخنة، رفع، عمل على مرتفعات، نظافة) إلخ", "Choice", "نعم,لا"),
                Create(26, "الملاحظات والتعليقات", "Text", "إدخال نص"),
                Create(27, "صور الموقع", "Upload", "تحميل ملف")
            };

            context.InspectionQuestions.AddRange(questions);
            context.SaveChanges();
        }

        public static void SeedUsers(AppDbContext context)
        {
            if (context.Users.Any())
            {
                return;
            }

            var users = new List<User>
            {
                new User { FullName = "المفتش العام", Email = "inspector@hse.com", Password = "123", Role = "Inspector", JobNumber = "1001", Department = "Safety" },
                new User { FullName = "مدير المسمع", Email = "supervisor@hse.com", Password = "123", Role = "Supervisor", JobNumber = "1002", Department = "Operations" },
                new User { FullName = "الموظف المثالي", Email = "employee@hse.com", Password = "123", Role = "Employee", JobNumber = "1003", Department = "Maintenance" },
                new User { FullName = "مستلم التقارير", Email = "recipient@hse.com", Password = "123", Role = "Recipient", JobNumber = "1004", Department = "Management" }
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }

        private static InspectionQuestion Create(int id, string text, string type, string options)
        {
            return new InspectionQuestion { Id = id, QuestionText = text, QuestionType = type, Options = options };
        }
    }
}
