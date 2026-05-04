using System.Collections.Generic;

public struct NicknameStorage
{
    public string Hostname;
    public Dictionary<string,string> Nicknames;
    public HashSet<string> CheckNicknames;

    public NicknameStorage Initialize(string hostname)
    {
        Hostname = hostname;
        Nicknames = new Dictionary<string,string>();
        CheckNicknames = new HashSet<string>();
        CheckNicknames.Add(hostname);
        CheckNicknames.Add("Empty");
        return this;
    }
}
