using System.Collections.Generic;
using System.Text;

namespace Sailwind;

partial class SailwindPanelComponent
{
	private void GeneratePointerEvents( StringBuilder sb )
	{
		GenerateUtility( sb, "pointer-events-auto", "pointer-events: auto" );
		GenerateUtility( sb, "pointer-events-none", "pointer-events: none" );
		GenerateUtility( sb, "pointer-events-all", "pointer-events: all" );
	}
}
