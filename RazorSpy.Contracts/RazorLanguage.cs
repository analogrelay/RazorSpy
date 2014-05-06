using System;
using System.CodeDom.Compiler;

namespace RazorSpy.Contracts
{
    public class RazorLanguage
    {
        public string Id { get; private set; }
        public string Name { get; private set; }

        public RazorLanguage(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public CodeDomProvider CreateCodeDomProvider()
        {
            if (!CodeDomProvider.IsDefinedLanguage(Id))
            {
                throw new NotSupportedException("Language not supported: " + Id);
            }
            return CodeDomProvider.GetCompilerInfo(Id).CreateProvider();
        }
    }
}
