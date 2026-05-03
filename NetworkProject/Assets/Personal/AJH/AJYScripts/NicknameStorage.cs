using System.Collections.Generic;

public struct NicknameStorage
{
    public string Hostname;
    public Dictionary<string,string> Nicknames;

    public NicknameStorage Initialize(string hostname)
    {
        Hostname = hostname;
        Nicknames = new Dictionary<string,string>();
        return this;
    }
}
