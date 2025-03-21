namespace Generator;

public partial class ConstantMethodGenerator
{
    internal class Emitter(string returnValue) : SyntaxDescriptionEmitter
    {
        public override Option<IEmitter> VisitMethodBody(MethodDescription description)
        {
            if (description.IsTargetNode is false)
            {
                return None;
            }
            
            Emitter.Append(" => ").Append(returnValue).Append(";").NewLine();

            return None;
        }
    }
}
