using System.Text;

namespace Sailwind;

partial class SailwindPanelComponent
{
	private void GenerateFonts( StringBuilder sb )
	{
		// todo: separate custom classes into a separate non-static stylesheet that gets regenerated
		// whenever the layout tree is changed
		var customClasses = FindCustomClasses( "font-[" );
		foreach ( var className in customClasses )
		{
			var start = className.IndexOf( '[' ) + 1;
			var end = className.IndexOf( ']' );

			var fontName = className[start..end];
			Log.Info( $"Found class {className} with font {fontName}" );
			GenerateUtility( sb, $"bg-[{fontName}]", $"font-family: \"{fontName}\"", includePointer: true );
		}
	}
}
