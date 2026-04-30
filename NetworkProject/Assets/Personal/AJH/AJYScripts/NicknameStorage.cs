using System.Collections.Generic;

public struct NicknameStorage
{
    public string Hostname;
    public List<string> Nicknames;

    public NicknameStorage Initialize(string hostname)
    {
        Hostname = hostname;
        Nicknames = new List<string>();
        return this;
    }
}
