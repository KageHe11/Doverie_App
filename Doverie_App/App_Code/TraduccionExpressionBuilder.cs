using System.CodeDom;
using System.Web.Compilation;
using System.Web.UI;

/// Permite traducir textos de controles directamente en el markup, por
/// ejemplo el mensaje de un validador:
///   ErrorMessage="&lt;%$ Traduccion:valid.obligatorio %&gt;"
/// Se evalúa en cada request con Traductor.T, así que sigue el idioma de la
/// sesión igual que el resto de la interfaz. Registrado en Web.config.
[ExpressionPrefix("Traduccion")]
public class TraduccionExpressionBuilder : ExpressionBuilder
{
    public override CodeExpression GetCodeExpression(BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
    {
        return new CodeMethodInvokeExpression(
            new CodeTypeReferenceExpression(typeof(Traductor)), "T", new CodePrimitiveExpression(entry.Expression.Trim()));
    }
}
