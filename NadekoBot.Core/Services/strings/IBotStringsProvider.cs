﻿namespace NadekoBot.Core.Services
{
    public interface IBotStringsProvider
    {
        string GetText(string langName, string key);
        void Reload();
    }
}