﻿namespace AlephNote.Common.Util
{
	public interface IThemeListener
	{
		void OnThemeChanged();
		bool IsTargetAlive { get; }
	}
}
