using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using Ardalis.ApiEndpointAnalyzer;

namespace Ardalis.ApiEndpointAnalyzer.Test
{
    [TestClass]
    public class AnalyzerTest : DiagnosticVerifier
    {
        [TestMethod]
        public void ShouldNotRaiseWarning_OnlyOneMethod()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public class TypeName : Ardalis.ApiEndpoints.BaseEndpoint<string>
        {   
            public void OneMethod()
            {
            }
        }
    }

    namespace Ardalis.ApiEndpoints
    {
        public class BaseEndpoint<A,B>
        {
        }
        public class BaseEndpoint<A>
        {
        }  
}
";
            VerifyCSharpDiagnostic(test);

        }

        [TestMethod]
        public void ShouldNotRaiseWarning_ConstructorNotConsideredPublicMethod()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public class TypeName : Ardalis.ApiEndpoints.BaseEndpoint<string>
        {   
            public TypeName()
            {
            }

            public void OneMethod()
            {
            }
        }
    }

    namespace Ardalis.ApiEndpoints
    {
        public class BaseEndpoint<A,B>
        {
        }
        public class BaseEndpoint<A>
        {
        }
    }
";
            VerifyCSharpDiagnostic(test);

        }

        [TestMethod]
        public void ShouldRaiseWarning_MoreThanOnePublicMethod()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public class TypeName : Ardalis.ApiEndpoints.BaseEndpoint<string>
        {   
            public void OneMethod()
            {
            }

            public void TwoMethod()
            {
            }

        }
    }

    namespace Ardalis.ApiEndpoints
    {
        public class BaseEndpoint<A,B>
        {
        }
        public class BaseEndpoint<A>
        {
        }
    }
";

            var expected = new DiagnosticResult
            {
                Id = "ArdalisApiEndpointAnalyzer",
                Message = String.Format("Type '{0}' inherits from ApiEndpoint and has more than one public method", "TypeName"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 11, 22)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);

        }


        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ArdalisApiEndpointAnalyzerAnalyzer();
        }

    }

}
