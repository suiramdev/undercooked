using System.Collections.Generic;
using System.Text;

namespace Sailwind;

partial class SailwindPanelComponent
{
	private readonly Dictionary<string, int> fontSizes = new()
	{
		["xs"] = 12,
		["sm"] = 14,
		["base"] = 16,
		["lg"] = 18,
		["xl"] = 20,
		["2xl"] = 24,
		["3xl"] = 30,
		["4xl"] = 36,
		["5xl"] = 48,
		["6xl"] = 60,
		["7xl"] = 72,
		["8xl"] = 96,
		["9xl"] = 128
	};

	private void GenerateFontUtilities( StringBuilder sb )
	{
		foreach ( var (name, size) in fontSizes )
		{
			GenerateUtility( sb, $"text-{name}", $"font-size: {size}px", includePointer: true );
		}

		// Font weights with hover
		GenerateUtility( sb, "font-normal", "font-weight: 400", includePointer: true );
		GenerateUtility( sb, "font-medium", "font-weight: 500", includePointer: true );
		GenerateUtility( sb, "font-bold", "font-weight: 700", includePointer: true );
	}

	private void GenerateTextTransforms( StringBuilder sb )
	{
		GenerateUtility( sb, "uppercase", "text-transform: uppercase", includePointer: true );
		GenerateUtility( sb, "lowercase", "text-transform: lowercase", includePointer: true );
		GenerateUtility( sb, "capitalize", "text-transform: capitalize", includePointer: true );
		GenerateUtility( sb, "normal-case", "text-transform: none", includePointer: true );
	}

	private void GenerateTextAligns( StringBuilder sb )
	{
		GenerateUtility( sb, "text-left", "text-align: left", includePointer: true );
		GenerateUtility( sb, "text-center", "text-align: center", includePointer: true );
		GenerateUtility( sb, "text-right", "text-align: right", includePointer: true );
		// GenerateUtility( sb, "text-justify", "text-align: justify", includePointer: true ); // Unsupported
		// GenerateUtility( sb, "text-start", "text-align: start", includePointer: true ); // Unsupported
		// GenerateUtility( sb, "text-end", "text-align: end", includePointer: true ); // Unsupported
	}
}
