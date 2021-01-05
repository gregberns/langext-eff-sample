using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace LangExtEffSample
{
    public class Person : Record<Person>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
