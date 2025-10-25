using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.UI;
using System.Collections.Generic;
using System.Text;

namespace Sailwind;

[Hide]
[Title( "Tailwind Panel" )]
public partial class SailwindPanelComponent : PanelComponent
{
	private void GenerateUtility( StringBuilder sb, string className, string cssProperties, bool includePointer = false )
	{
		sb.AppendLine( $".{className} {{ {cssProperties}; }}" );
		if ( includePointer )
		{
			sb.AppendLine( $".hover-{className}:hover {{ {cssProperties}; }}" );
			sb.AppendLine( $".active-{className}:active {{ {cssProperties}; }}" );
		}
	}

	private List<string> FindCustomClasses( string prefix )
	{
		List<string> matches = new();

		void EnumerateChildren( Panel root )
		{
			foreach ( var panel in root.Children )
			{
				foreach ( var className in panel.Class )
				{
					if ( className.StartsWith( prefix ) && !matches.Contains( className ) )
					{
						matches.Add( className );
					}

					if ( className.StartsWith( "hover-" + prefix ) && !matches.Contains( className ) )
					{
						matches.Add( className );
					}
				}

				EnumerateChildren( panel );
			}
		}

		EnumerateChildren( Panel );

		return matches;
	}

	public string Generate()
	{
		var sb = new StringBuilder();

		GenerateSpacingUtilities( sb );
		GenerateColorUtilities( sb );
		GenerateFontUtilities( sb );
		GenerateFlexUtilities( sb );
		GenerateShadowUtilities( sb );
		GenerateRoundingUtilities( sb );
		GenerateTransitionUtilities( sb );
		GenerateSizeUtilities( sb );
		GenerateTransformUtilities( sb );
		GeneratePositionUtilities( sb );
		GenerateTextTransforms( sb );
		GenerateOpacityUtilities( sb );
		GeneratePointerEvents( sb );
		GenerateTextAligns( sb );
		GenerateFonts( sb );

		return sb.ToString();
	}

	protected override void OnTreeFirstBuilt()
	{
		string styleCode;

		{
			var timer = FastTimer.StartNew();

			styleCode = Generate();

			Log.Trace( $"Generating Sailwind stylesheet took {timer.ElapsedSeconds:0.00}s" );
		}

		{
			var timer = FastTimer.StartNew();

			Panel.StyleSheet.Parse( styleCode );

			Log.Trace( $"Parsing Sailwind stylesheet took {timer.ElapsedSeconds:0.00}s" );
		}
	}
}
