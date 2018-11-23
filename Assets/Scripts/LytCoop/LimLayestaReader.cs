using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Lanotalium;
using Lanotalium.Chart;
using UnityEngine;

public class LimLayestaReader
{
    public static bool Validate(string path)
    {
        if (!lzip.validateFile(path)) return false;
        if (!lzip.entryExists(path, "info.bytes")) return false;
        try
        {
            byte[] buf = lzip.entry2Buffer(path, "info.bytes");
            MemoryStream ms = new MemoryStream(buf);
            BinaryReader br = new BinaryReader(ms);
            br.ReadString();
            br.ReadString();
            int length = br.ReadInt32();
            for (int i = 0; i < length; ++i)
            {
                br.ReadString();
            }
            br.ReadInt32();
            br.ReadInt32();
            if (br.BaseStream.Length > br.BaseStream.Position)
            {
                br.ReadString();
            }
            if (br.BaseStream.Length > br.BaseStream.Position)
            {
                br.ReadInt32();
            }
            br.Close();
            ms.Close();
        }
        catch (Exception)
        {
            return false;
        }
        if (!lzip.entryExists(path, "music.mp3")) return false;
        if (!lzip.entryExists(path, "background.jpg")) return false;

        return true;
    }
    public static byte[] LoadBackgroundImage(string path)
    {
        if (!lzip.entryExists(path, "background.jpg")) return null;
        return lzip.entry2Buffer(path, "background.jpg");
    }
}