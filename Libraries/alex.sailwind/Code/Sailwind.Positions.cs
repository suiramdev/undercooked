using System.Collections.Generic;
using System.Text;

namespace Sailwind;

partial class SailwindPanelComponent
{
	private void GeneratePositionUtilities( StringBuilder sb )
	{
		foreach ( var (key, value) in sizes )
		{
			GenerateUtility( sb, $"top-{key}", $"top: {value}" );
			GenerateUtility( sb, $"bottom-{key}", $"bottom: {value}" );
			GenerateUtility( sb, $"left-{key}", $"left: {value}" );
			GenerateUtility( sb, $"right-{key}", $"right: {value}" );
			// GenerateUtility( sb, $"inset-{key}", $"inset: {value}" ); // Unsupported
		}

		GenerateUtility( sb, $"relative", $"position: relative" );
		GenerateUtility( sb, $"absolute", $"position: absolute" );

		var percentages = new[] { "1/2", "1/3", "2/3", "1/4", "3/4", "1/5", "2/5", "3/5", "4/5" };
		var fractions = new[] { 0.5f, 0.333f, 0.667f, 0.25f, 0.75f, 0.2f, 0.4f, 0.6f, 0.8f };

		for ( var i = 0; i < percentages.Length; i++ )
		{
			GenerateUtility( sb, $"top-{percentages[i]}", $"top: {fractions[i] * 100}%" );
			GenerateUtility( sb, $"bottom-{percentages[i]}", $"bottom: {fractions[i] * 100}%" );
			GenerateUtility( sb, $"left-{percentages[i]}", $"left: {fractions[i] * 100}%" );
			GenerateUtility( sb, $"right-{percentages[i]}", $"right: {fractions[i] * 100}%" );
			// GenerateUtility( sb, $"inset-{percentages[i]}", $"inset: {fractions[i] * 100}%" ); // Unsupported
		}
	}
}
