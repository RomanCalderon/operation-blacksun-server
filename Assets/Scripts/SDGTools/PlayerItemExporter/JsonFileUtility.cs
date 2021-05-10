using System.IO;

public static class JsonFileUtility
{
    public static bool WriteToFile ( string path, string json )
    {
        if ( string.IsNullOrEmpty ( path ) || string.IsNullOrEmpty ( json ) )
        {
            return false;
        }
        using StreamWriter stream = File.CreateText ( path );
        stream.Write ( json );
        stream.Close ();
        return true;
    }

    public static bool ReadFromFile ( string path, out string json )
    {
        if ( string.IsNullOrEmpty ( path ) )
        {
            json = null;
            return false;
        }
        if ( !File.Exists ( path ) )
        {
            json = null;
            return false;
        }
        using StreamReader stream = new StreamReader ( path );
        json = stream.ReadToEnd ();
        stream.Close ();
        return true;
    }
}
