using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;

namespace eXpand.Persistent.Base.General
{
    public interface ILogicRule
    {
        string ID { get; set; }
        string ExecutionContextGroup { get; set; }
        ViewType ViewType { get; set; }

        /// <summary>
        /// Nesting of ListView, but not works in MainWindow...(Root)...
        /// </summary>
        Nesting Nesting { get; set; }
        

        string Description { get; set; }

        ITypeInfo TypeInfo { get; set; }

        string ViewId { get; set; }
        int Index { get; set; }
        
    }
}