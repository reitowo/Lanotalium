using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if (UNITY_WSA_8_1 ||  UNITY_WP_8_1 || UNITY_WINRT_8_1) && !UNITY_EDITOR
 using File = UnityEngine.Windows.File;
 #else
 using File = System.IO.File;
 #endif

#if NETFX_CORE
    #if UNITY_WSA_10_0
        using System.IO.IsolatedStorage;
        using static System.IO.Directory;
        using static System.IO.File;
        using static System.IO.FileStream;
    #endif
#endif


public class lzip
{
#if !UNITY_WEBPLAYER  || UNITY_EDITOR

#if !UNITY_WSA || UNITY_EDITOR
// A struct that is used for the low level functions
// Holds info about each entry in a zip archive.
public struct fileStat {
	public int index;

	public int compSize;
	public int uncompSize;

	public int nameSize;
	public string name;

	public int commentSize;
	public string comment;

	public bool isDirectory;
	public bool isSupported;
	public bool isEncrypted;
}
#endif


#if (UNITY_IPHONE || UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR

	#if (UNITY_IPHONE || UNITY_IOS) && !UNITY_WEBGL
		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		private static extern int zsetPermissions(string filePath, string _user, string _group, string _other);

		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool zipValidateFile(string zip_Archive, IntPtr FileBuffer,int fileBufferLength);

		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		private static extern int zipGetTotalFiles(string zipArchive, IntPtr FileBuffer, int fileBufferLength);
		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		private static extern int zipGetTotalEntries(string zipArchive, IntPtr FileBuffer, int fileBufferLength);
		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		private static extern int zipGetInfoA(string zipArchive, IntPtr total, IntPtr FileBuffer, int fileBufferLength);
		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr zipGetInfo(string zipArchive, int size, IntPtr unc, IntPtr comp, IntPtr FileBuffer, int fileBufferLength);

		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		private static extern int zipGetEntrySize(string zipArchive, string entry, IntPtr FileBuffer, int fileBufferLength);
		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool zipEntryExists(string zipArchive, string entry, IntPtr FileBuffer, int fileBufferLength);

		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		private static extern int zipCD(int levelOfCompression, string zipArchive, string inFilePath, string fileName, string comment, [MarshalAs(UnmanagedType.LPStr)]  string password);

		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool zipBuf2File(int levelOfCompression, string zipArchive, string arc_filename, IntPtr buffer, int bufferSize, string comment,[MarshalAs(UnmanagedType.LPStr)]  string password);
		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		private static extern int zipDeleteFile(string zipArchive, string arc_filename, string tempArchive);
		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		private static extern int zipEntry2Buffer(string zipArchive, string entry, IntPtr buffer, int bufferSize, IntPtr FileBuffer, int fileBufferLength, [MarshalAs(UnmanagedType.LPStr)] string password);

		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		private static extern int zipEX(string zipArchive, string outPath, IntPtr progress, IntPtr FileBuffer, int fileBufferLength, IntPtr proc, [MarshalAs(UnmanagedType.LPStr)] string password);
		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		private static extern int zipEntry(string zipArchive, string arc_filename, string outpath, IntPtr FileBuffer,int fileBufferLength, IntPtr proc, [MarshalAs(UnmanagedType.LPStr)] string password);
		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		private static extern uint getEntryDateTime(string zipArchive, string arc_filename, IntPtr FileBuffer,int fileBufferLength);
	#endif
	#if (UNITY_IPHONE || UNITY_IOS || UNITY_TVOS || UNITY_WEBGL)
		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		private static extern void releaseBuffer(IntPtr buffer);
		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr zipCompressBuffer(IntPtr source, int sourceLen, int levelOfCompression, ref int v);
		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr zipDecompressBuffer(IntPtr source, int sourceLen, ref int v);

		//gzip section
		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int zipGzip(IntPtr source, int sourceLen, IntPtr outBuffer, int levelOfCompression, bool addHeader, bool addFooter);
		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int zipUnGzip(IntPtr source, int sourceLen, IntPtr outBuffer, int outLen, bool hasHeader, bool hasFooter);
		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int zipUnGzip2(IntPtr source, int sourceLen, IntPtr outBuffer, int outLen);
	#endif
#endif

#if UNITY_5_4_OR_NEWER
#if (UNITY_ANDROID || UNITY_STANDALONE_LINUX || UNITY_WEBGL) && !UNITY_EDITOR || UNITY_EDITOR_LINUX
		private const string libname = "zipw";
#elif UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_WP8_1 || UNITY_WSA
		private const string libname = "libzipw";
#endif
#else
#if (UNITY_ANDROID || UNITY_STANDALONE_LINUX || UNITY_WEBGL) && !UNITY_EDITOR
		private const string libname = "zipw";
#endif
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_WP8_1 || UNITY_WSA
	private const string libname = "libzipw";
	#endif
#endif

#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_WP8_1 || UNITY_WSA || UNITY_ANDROID || UNITY_STANDALONE_LINUX
#if (!UNITY_WEBGL || UNITY_EDITOR)
	#if (UNITY_STANDALONE_OSX  || UNITY_STANDALONE_LINUX || UNITY_ANDROID || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX)&& !UNITY_EDITOR_WIN
		[DllImport(libname, EntryPoint = "zsetPermissions"
		#if (UNITY_ANDROID && !UNITY_EDITOR)
		, CallingConvention = CallingConvention.Cdecl
		#endif		
		)]
		internal static extern int zsetPermissions(string filePath, string _user, string _group, string _other);
	#endif

	//Windows/WSA10 only function to set encoding of reading/writing files
	//CP_ACP = 0
	//CP_OEMCP/UNICODE = 1
	//CP_UTF8 = 65001
	//CP_WINUNICODE = 1200

	//Default is CP_UTF8 = 65001
	#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA)
		[DllImport(libname, EntryPoint = "setEncoding"
		#if UNITY_WSA
		, CallingConvention = CallingConvention.Cdecl
		#endif
		#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
		,  CharSet = CharSet.Auto
		#endif
		#if UNITY_WSA && !UNITY_EDITOR
		,  CharSet = CharSet.Unicode
		#endif
		)]
		public static extern bool setEncoding(uint encoding);
	#endif

    [DllImport(libname, EntryPoint = "zipValidateFile"
	#if UNITY_WSA
	, CallingConvention = CallingConvention.Cdecl
	#endif
	#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
	,  CharSet = CharSet.Auto
	#endif
	#if UNITY_WSA && !UNITY_EDITOR
	,  CharSet = CharSet.Unicode
	#endif
	)]
    internal static extern bool zipValidateFile(string zipArchive, IntPtr FileBuffer,int fileBufferLength);

	[DllImport(libname, EntryPoint = "zipGetTotalFiles"
	#if UNITY_WSA
	, CallingConvention = CallingConvention.Cdecl
	#endif
	#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
	,  CharSet = CharSet.Auto
	#endif
	#if UNITY_WSA && !UNITY_EDITOR
	,  CharSet = CharSet.Unicode
	#endif
	)]
    internal static extern int zipGetTotalFiles(string zipArchive, IntPtr FileBuffer, int fileBufferLength);


	[DllImport(libname, EntryPoint = "zipGetTotalEntries"
	#if UNITY_WSA
	, CallingConvention = CallingConvention.Cdecl
	#endif
	#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
	,  CharSet = CharSet.Auto
	#endif
	#if UNITY_WSA && !UNITY_EDITOR
	,  CharSet = CharSet.Unicode
	#endif
	)]
    internal static extern int zipGetTotalEntries(string zipArchive, IntPtr FileBuffer, int fileBufferLength);


    [DllImport(libname, EntryPoint = "zipGetInfoA"
	#if UNITY_WSA
	, CallingConvention = CallingConvention.Cdecl
	#endif
	#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN) 
	,  CharSet = CharSet.Auto
	#endif
	#if UNITY_WSA && !UNITY_EDITOR
	,  CharSet = CharSet.Unicode
	#endif
	)]
	internal static extern int zipGetInfoA(string zipArchive,  IntPtr total, IntPtr FileBuffer, int fileBufferLength);



	#if !UNITY_WSA || UNITY_EDITOR
		[DllImport(libname, EntryPoint = "zipGetInfo"
		#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN) 
		,  CharSet = CharSet.Auto
		#endif
		)]
		internal static extern IntPtr zipGetInfo(string zipArchive, int size, IntPtr unc, IntPtr comp, IntPtr FileBuffer, int fileBufferLength);
	#else
		[DllImport(libname, EntryPoint = "zipGetInfo" , CallingConvention = CallingConvention.Cdecl
		#if UNITY_WSA && !UNITY_EDITOR
		,  CharSet = CharSet.Unicode
		#endif
		)]
		internal static extern int zipGetInfo(string zipArchive, IntPtr cbuffer, IntPtr unc, IntPtr comp,IntPtr FileBuffer, int fileBufferLength);
	#endif

    [DllImport(libname, EntryPoint = "releaseBuffer"
	#if UNITY_WSA
	, CallingConvention = CallingConvention.Cdecl
	#endif	
	)]
    internal static extern void releaseBuffer(IntPtr buffer);

    [DllImport(libname, EntryPoint = "zipGetEntrySize"
	#if UNITY_WSA
	, CallingConvention = CallingConvention.Cdecl
	#endif
	#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
	,  CharSet = CharSet.Auto
	#endif
	#if UNITY_WSA && !UNITY_EDITOR
	,  CharSet = CharSet.Unicode
	#endif
	)]
    internal static extern int zipGetEntrySize(string zipArchive, string entry, IntPtr FileBuffer, int fileBufferLength);

    [DllImport(libname, EntryPoint = "zipEntryExists"
	#if UNITY_WSA
	, CallingConvention = CallingConvention.Cdecl
	#endif
	#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
	,  CharSet = CharSet.Auto
	#endif
	#if UNITY_WSA && !UNITY_EDITOR
	,  CharSet = CharSet.Unicode
	#endif
	)]
    internal static extern bool zipEntryExists(string zipArchive, string entry, IntPtr FileBuffer, int fileBufferLength);



    [DllImport(libname, EntryPoint = "zipCD"
	#if UNITY_WSA
	, CallingConvention = CallingConvention.Cdecl
	#endif
	#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
	,  CharSet = CharSet.Auto
	#endif
	#if UNITY_WSA && !UNITY_EDITOR
	,  CharSet = CharSet.Unicode
	#endif
	)]
    internal static extern int zipCD(int levelOfCompression, string zipArchive, string inFilePath, string fileName, string comment, [MarshalAs(UnmanagedType.LPStr)]  string password
	#if !UNITY_EDITOR_OSX && !UNITY_STANDALONE_OSX && !UNITY_IOS && !UNITY_TVOS
	, bool useBz2
	#endif
	);


    [DllImport(libname, EntryPoint = "zipBuf2File"
	#if UNITY_WSA
	, CallingConvention = CallingConvention.Cdecl
	#endif
	#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
	,  CharSet = CharSet.Auto
	#endif
	#if UNITY_WSA && !UNITY_EDITOR
	,  CharSet = CharSet.Unicode
	#endif
	)]
    internal static extern bool zipBuf2File(int levelOfCompression, string zipArchive, string arc_filename, IntPtr buffer, int bufferSize, string comment,[MarshalAs(UnmanagedType.LPStr)]  string password
	#if !UNITY_EDITOR_OSX && !UNITY_STANDALONE_OSX && !UNITY_IOS && !UNITY_TVOS
	, bool useBz2
	#endif
	);



    [DllImport(libname, EntryPoint = "zipDeleteFile"
	#if UNITY_WSA
	, CallingConvention = CallingConvention.Cdecl
	#endif
	#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
	,  CharSet = CharSet.Auto
	#endif
	#if UNITY_WSA && !UNITY_EDITOR
	,  CharSet = CharSet.Unicode
	#endif
	)]
    internal static extern int zipDeleteFile(string zipArchive, string arc_filename, string tempArchive);


    [DllImport(libname, EntryPoint = "zipEntry2Buffer"
	#if UNITY_WSA
	, CallingConvention = CallingConvention.Cdecl
	#endif
	#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
	,  CharSet = CharSet.Auto
	#endif
	#if UNITY_WSA && !UNITY_EDITOR
	,  CharSet = CharSet.Unicode
	#endif
	)]
    internal static extern int zipEntry2Buffer(string zipArchive, string entry, IntPtr buffer, int bufferSize, IntPtr FileBuffer, int fileBufferLength, [MarshalAs(UnmanagedType.LPStr)] string password);



    [DllImport(libname, EntryPoint = "zipCompressBuffer"
	#if UNITY_WSA
	, CallingConvention = CallingConvention.Cdecl
	#endif	
	)]
    internal static extern IntPtr zipCompressBuffer(IntPtr source, int sourceLen, int levelOfCompression, ref int v);

    [DllImport(libname, EntryPoint = "zipDecompressBuffer"
	#if UNITY_WSA
	, CallingConvention = CallingConvention.Cdecl
	#endif	
	)]
    internal static extern IntPtr zipDecompressBuffer(IntPtr source, int sourceLen, ref int v);

    [DllImport(libname, EntryPoint = "zipEX"
	#if UNITY_WSA
	, CallingConvention = CallingConvention.Cdecl
	#endif
	#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
	,  CharSet = CharSet.Auto
	#endif
	#if UNITY_WSA && !UNITY_EDITOR
	,  CharSet = CharSet.Unicode
	#endif
	)]
	internal static extern int zipEX(string zipArchive, string outPath, IntPtr progress, IntPtr FileBuffer, int fileBufferLength, IntPtr proc, [MarshalAs(UnmanagedType.LPStr)] string password);



    [DllImport(libname, EntryPoint = "zipEntry"
	#if UNITY_WSA
	, CallingConvention = CallingConvention.Cdecl
	#endif
	#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
	,  CharSet = CharSet.Auto
	#endif
	#if UNITY_WSA && !UNITY_EDITOR
	,  CharSet = CharSet.Unicode
	#endif
	)]
    internal static extern int zipEntry(string zipArchive, string arc_filename, string outpath, IntPtr FileBuffer,int fileBufferLength, IntPtr proc, [MarshalAs(UnmanagedType.LPStr)] string password);


    [DllImport(libname, EntryPoint = "getEntryDateTime"
	#if UNITY_WSA
	, CallingConvention = CallingConvention.Cdecl
	#endif
	#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
	,  CharSet = CharSet.Auto
	#endif
	#if UNITY_WSA && !UNITY_EDITOR
	,  CharSet = CharSet.Unicode
	#endif
	)]
    internal static extern uint getEntryDateTime(string zipArchive, string arc_filename, IntPtr FileBuffer,int fileBufferLength);

	//gzip section
    [DllImport(libname, EntryPoint = "zipGzip"
	#if UNITY_WSA
	, CallingConvention = CallingConvention.Cdecl
	#endif	
	)]
    internal static extern int zipGzip(IntPtr source, int sourceLen, IntPtr outBuffer, int levelOfCompression, bool addHeader, bool addFooter);

	#endif

    [DllImport(libname, EntryPoint = "zipUnGzip"
	#if UNITY_WSA
	, CallingConvention = CallingConvention.Cdecl
	#endif	
	)]

    internal static extern int zipUnGzip(IntPtr source, int sourceLen, IntPtr outBuffer,int outLen, bool hasHeader, bool hasFooter);
    [DllImport(libname, EntryPoint = "zipUnGzip2"
	#if UNITY_WSA
	, CallingConvention = CallingConvention.Cdecl
	#endif	
	)]
    internal static extern int zipUnGzip2(IntPtr source, int sourceLen, IntPtr outBuffer, int outLen);

	//WSA81 only functions
	#if UNITY_WSA && !UNITY_EDITOR
		[DllImport(libname, EntryPoint = "FileRename", CallingConvention = CallingConvention.Cdecl,  CharSet = CharSet.Unicode)]
		internal static extern bool fileRename(string src, string dst);

		[DllImport(libname, EntryPoint = "FileCopy", CallingConvention = CallingConvention.Cdecl,  CharSet = CharSet.Unicode)]
		public static extern bool fileCopy(string src, string dst);
	#endif

#endif




#if !UNITY_WEBGL || UNITY_EDITOR
	// set permissions of a file in user, group, other.
	// Each string should contain any or all chars of "rwx".
	// returns 0 on success
	//
	public static int setFilePermissions(string filePath, string _user, string _group, string _other){
		#if (UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX || UNITY_ANDROID || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR_WIN
			return zsetPermissions(filePath, _user, _group, _other);
		#else
			return -1;
		#endif
	}



	// A function that will validate a zip archive.
	//
    // zipArchive       : the zip to be checked
	// FileBuffer		: A buffer that holds a zip file. When assigned the function will read from this buffer and will ignore the filePath.
    // ERROR CODES
	//
	//					: true. The archive is ok.
	//					: false. The archive could not be validated.
	//
    public static bool validateFile(string zipArchive, byte[] FileBuffer = null) {
		bool res = false;
		
		#if (UNITY_IPHONE || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_STANDALONE_LINUX || UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_WSA)
			if(FileBuffer!= null) {
				GCHandle fbuf = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);
				res = zipValidateFile(null, fbuf.AddrOfPinnedObject(), FileBuffer.Length);
				fbuf.Free();
				return res;
			}
		#endif

        res = zipValidateFile(@zipArchive, IntPtr.Zero, 0);

		return res;
    }


	

    // A function that returns the total number of files in a zip archive (files only, no folders).
    //
    // zipArchive       : the zip to be checked
	// FileBuffer		: A buffer that holds a zip file. When assigned the function will read from this buffer and will ignore the filePath.
    //
    // ERROR CODES
    //                  : -1 = failed to access zip archive
    //                  :  any number>0 = the number of files in the zip archive
    //
    public static int getTotalFiles(string zipArchive, byte[] FileBuffer = null) {
		int res;
		
		#if (UNITY_IPHONE || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_STANDALONE_LINUX || UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_WSA)
			if(FileBuffer!= null) {
				GCHandle fbuf = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);
				res = zipGetTotalFiles(null, fbuf.AddrOfPinnedObject(), FileBuffer.Length);

				fbuf.Free();
				return res;
			}
		#endif

        res = zipGetTotalFiles(@zipArchive, IntPtr.Zero, 0);

		return res;
    }

	// A function that will return the total entries in a zip arcive. (files + folders)
	//
    // zipArchive       : the zip to be checked
	// FileBuffer		: A buffer that holds a zip file. When assigned the function will read from this buffer and will ignore the filePath.
    //
    // ERROR CODES
    //                  : -2 = failed to access zip archive
    //                  :  any number>0 = the number of entries in the zip archive
    //
    public static int getTotalEntries(string zipArchive, byte[] FileBuffer = null) {
		int res;
		
		#if (UNITY_IPHONE || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_STANDALONE_LINUX || UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_WSA)
			if(FileBuffer!= null) {
				GCHandle fbuf = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);
				res = zipGetTotalEntries(null, fbuf.AddrOfPinnedObject(), FileBuffer.Length);

				fbuf.Free();
				return res;
			}
		#endif

        res = zipGetTotalEntries(@zipArchive, IntPtr.Zero, 0);

		return res;
    }

    
    // Lists get filled with filenames (including path if the file is in a folder) and uncompressed file sizes
    // Call getFileInfo(string zipArchive, string path) to get them filled. After that you can iterate through them to get the info you want.
    public static List<string> ninfo = new List<string>();//filenames
    public static List<long> uinfo = new List<long>();//uncompressed file sizes
    public static List<long> cinfo = new List<long>();//compressed file sizes
    public static int zipFiles, zipFolders;// global integers that store the number of files and folders in a zip file.

    // This function fills the Lists with the filenames and file sizes that are in the zip file
    // Returns			: the total size of uncompressed bytes of the files in the zip archive 
    //
    // zipArchive		: the full path to the archive, including the archives name. (/myPath/myArchive.zip)
    // path             : this is no longer used. It is kept for a while for backwards compatibility.
	// FileBuffer		: A buffer that holds a zip file. When assigned the function will read from this buffer and will ignore the filePath.
    //
    // ERROR CODES      : -1 = Input file not found
    //                  : -2 = Could not get info
    //
    public static long getFileInfo(string zipArchive, string path = null, byte[] FileBuffer = null) {
        ninfo.Clear(); uinfo.Clear(); cinfo.Clear();
        zipFiles = 0; zipFolders = 0;

		int res;

		int[] tt = new int[1];
		GCHandle tb = GCHandle.Alloc(tt, GCHandleType.Pinned);

		#if (UNITY_IPHONE || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_STANDALONE_LINUX || UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_WSA)
			if(FileBuffer!= null) {
				GCHandle fbuf = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);
				res = zipGetInfoA(null,tb.AddrOfPinnedObject(), fbuf.AddrOfPinnedObject(), FileBuffer.Length);
				fbuf.Free();
			} else {
				res = zipGetInfoA(@zipArchive, tb.AddrOfPinnedObject(),IntPtr.Zero, 0);
			}
		#else
			res = zipGetInfoA(@zipArchive, tb.AddrOfPinnedObject(),IntPtr.Zero, 0);
		#endif
		tb.Free();


        if (res <=0) {  return -1; }

		IntPtr uni = IntPtr.Zero;

		uint[] unc = new uint[tt[0]];
		uint[] comp = new uint[tt[0]];

		GCHandle uncb = GCHandle.Alloc(unc, GCHandleType.Pinned);
		GCHandle compb = GCHandle.Alloc(comp, GCHandleType.Pinned);

		#if UNITY_WSA && !UNITY_EDITOR
			byte[] cbuffer = new byte[res+1];
			GCHandle cbuf = GCHandle.Alloc(cbuffer, GCHandleType.Pinned);
			int un = 0;
		#endif


		#if (UNITY_IPHONE || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_STANDALONE_LINUX || UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_WSA)
			#if !UNITY_WSA || UNITY_EDITOR
			if(FileBuffer!= null) {
				GCHandle fbuf = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);
				uni = zipGetInfo(null, res, uncb.AddrOfPinnedObject(), compb.AddrOfPinnedObject(), fbuf.AddrOfPinnedObject() ,FileBuffer.Length);
				fbuf.Free();
			} else {
				uni = zipGetInfo(@zipArchive, res, uncb.AddrOfPinnedObject(), compb.AddrOfPinnedObject(),IntPtr.Zero, 0);
			}
			#else
			if(FileBuffer!= null) {
				GCHandle fbuf = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);
				un = zipGetInfo(null,  cbuf.AddrOfPinnedObject(), uncb.AddrOfPinnedObject(), compb.AddrOfPinnedObject(),fbuf.AddrOfPinnedObject() ,FileBuffer.Length);
				fbuf.Free();
			} else {
				un = zipGetInfo(@zipArchive, cbuf.AddrOfPinnedObject(), uncb.AddrOfPinnedObject(), compb.AddrOfPinnedObject(),IntPtr.Zero, 0);
			}
			cbuf.Free();
			#endif
		#else
			uni = zipGetInfo(@zipArchive, res, uncb.AddrOfPinnedObject(), compb.AddrOfPinnedObject(), IntPtr.Zero, 0);
		#endif


		
		#if !UNITY_WSA || UNITY_EDITOR
			if(uni == IntPtr.Zero) { uncb.Free(); compb.Free(); return -2; }
		#else
			if(un != 1) { cbuffer=null; uncb.Free(); compb.Free(); return-2;}
		#endif
		

		#if !UNITY_WSA || UNITY_EDITOR
			string str = Marshal.PtrToStringAuto ( uni );
		#else
			#if UNITY_WSA_10_0
				string str = UnicodeEncoding.UTF8.GetString(cbuffer);
			#else
				string str = ASCIIEncoding.UTF8.GetString(cbuffer);
			#endif
		#endif


		StringReader r = new StringReader(str);

        string line;
		long sum = 0;


		for(int i=0; i<tt[0]; i++) {

			if( ( line = r.ReadLine() ) != null)  ninfo.Add(line); 

			if(unc != null) {
				uinfo.Add((long)unc[i]);
				sum += unc[i];
				if (unc[i] > 0) zipFiles++; else zipFolders++;
			}

			if(comp != null) cinfo.Add((long)comp[i]);
			
		}

		#if !NETFX_CORE
        r.Close(); 
		#endif

        r.Dispose();

		uncb.Free(); compb.Free();

		#if !UNITY_WSA || UNITY_EDITOR
			releaseBuffer(uni);
		#else
			cbuffer = null;
		#endif

		tt = null;
		unc = null;
		comp = null;

        return sum;
    }




    // A function that returns the uncompressed size of a file in a zip archive.
    //
    // zipArchive    : the zip archive to get the info from.
    // entry         : the entry for which we want to know it uncompressed size.
	// FileBuffer	 : A buffer that holds a zip file. When assigned the function will read from this buffer and will ignore the filePath. 
    //
    public static int getEntrySize(string zipArchive, string entry, byte[] FileBuffer = null) {
		int res;
		#if (UNITY_IPHONE || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_STANDALONE_LINUX || UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_WSA)
			if(FileBuffer!= null) {
				GCHandle fbuf = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);
				res = zipGetEntrySize(null, entry, fbuf.AddrOfPinnedObject(), FileBuffer.Length);

				fbuf.Free();
				return res;
			}
		#endif

        res = zipGetEntrySize(@zipArchive, entry, IntPtr.Zero, 0);

		return res;
    }


	// A function that tells if an entry in zip archive exists.
	//
	// Returns true or false.
	//
    // zipArchive    : the zip archive to get the info from.
    // entry         : the entry for which we want to know if it exists.
	// FileBuffer	 : A buffer that holds a zip file. When assigned the function will read from this buffer and will ignore the filePath.
	//
    public static bool entryExists(string zipArchive, string entry, byte[] FileBuffer = null) {
		bool res;
		#if (UNITY_IPHONE || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_STANDALONE_LINUX || UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_WSA)
			if(FileBuffer!= null) {
				GCHandle fbuf = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);
				res = zipEntryExists(null, entry, fbuf.AddrOfPinnedObject(), FileBuffer.Length);

				fbuf.Free();
				return res;
			}
		#endif

        res = zipEntryExists(@zipArchive, entry, IntPtr.Zero, 0);

		return res;
    }

    // A function that compresses a byte buffer to a zlib stream compressed buffer. Provide a reference buffer to write to. This buffer will be resized.
    //
    // source                : the input buffer
    // outBuffer             : the referenced output buffer
    // levelOfCompression    : (0-10) recommended 9 for maximum (10 is highest but slower and not zlib compatible)
    //
    // ERROR CODES   : true  = success
    //               : false = failed
    //
    public static bool compressBuffer(byte[] source, ref byte[] outBuffer, int levelOfCompression) {
        if (levelOfCompression < 0) levelOfCompression = 0; if (levelOfCompression > 10) levelOfCompression = 10;

        GCHandle sbuf = GCHandle.Alloc(source, GCHandleType.Pinned);
        IntPtr ptr;
        int siz = 0;

        ptr = zipCompressBuffer(sbuf.AddrOfPinnedObject(), source.Length, levelOfCompression, ref siz);

        if (siz == 0 || ptr == IntPtr.Zero) { sbuf.Free(); releaseBuffer(ptr); return false; }

        System.Array.Resize(ref outBuffer, siz);
        Marshal.Copy(ptr, outBuffer, 0, siz);

        sbuf.Free();
        releaseBuffer(ptr);

        return true;
    }



	// same as the compressBuffer function, only this function will put the result in a fixed size buffer to avoid memory allocations.
	// the compressed size is returned so you can manipulate it at will.
	//
	// safe: if set to true the function will abort if the compressed resut is larger the the fixed size output buffer.
	// Otherwise compressed data will be written only until the end of the fixed output buffer.
	//
	public static int compressBufferFixed(byte[] source, ref byte[] outBuffer, int levelOfCompression, bool safe = true) {
        if (levelOfCompression < 0) levelOfCompression = 0; if (levelOfCompression > 10) levelOfCompression = 10;

        GCHandle sbuf = GCHandle.Alloc(source, GCHandleType.Pinned);
        IntPtr ptr;
        int siz = 0;

        ptr = zipCompressBuffer(sbuf.AddrOfPinnedObject(), source.Length, levelOfCompression, ref siz);

        if (siz == 0 || ptr == IntPtr.Zero) { sbuf.Free(); releaseBuffer(ptr); return 0; }

		if (siz>outBuffer.Length) {
			if(safe) { sbuf.Free(); releaseBuffer(ptr); return 0; } else { siz = outBuffer.Length; }
		}

        Marshal.Copy(ptr, outBuffer, 0, siz);

        sbuf.Free();
        releaseBuffer(ptr);

        return siz;
    }



    // A function that compresses a byte buffer to a zlib stream compressed buffer. Returns a new buffer with the compressed data.
    //
    // source                : the input buffer
    // levelOfCompression    : (0-10) recommended 9 for maximum (10 is highest but slower and not zlib compatible)
    //
    // ERROR CODES           : a valid byte buffer = success
    //                       : null                = failed
    //
    public static byte[] compressBuffer(byte[] source,  int levelOfCompression) {
        if (levelOfCompression < 0) levelOfCompression = 0; if (levelOfCompression > 10) levelOfCompression = 10;

        GCHandle sbuf = GCHandle.Alloc(source, GCHandleType.Pinned);
        IntPtr ptr;
        int siz = 0;

        ptr = zipCompressBuffer(sbuf.AddrOfPinnedObject(), source.Length, levelOfCompression, ref siz);

        if (siz == 0 || ptr == IntPtr.Zero) { sbuf.Free(); releaseBuffer(ptr); return null; }

        byte[] buffer = new byte[siz];
        Marshal.Copy(ptr, buffer, 0, siz);

        sbuf.Free();
        releaseBuffer(ptr);

        return buffer;
    }



    // A function that decompresses a zlib compressed buffer to a referenced outBuffer. The outbuffer will be resized.
    //
    // source            : a zlib compressed buffer.
    // outBuffer         : a referenced out buffer provided to extract the data. This buffer will be resized to fit the uncompressed data.
    //
    // ERROR CODES       : true  = success
    //                   : false = failed
    //
    public static bool decompressBuffer(byte[] source, ref byte[] outBuffer) {
        GCHandle sbuf = GCHandle.Alloc(source, GCHandleType.Pinned);
        IntPtr ptr;
        int siz = 0;

        ptr = zipDecompressBuffer(sbuf.AddrOfPinnedObject(), source.Length, ref siz);

        if (siz == 0 || ptr == IntPtr.Zero) { sbuf.Free(); releaseBuffer(ptr); return false; }

        System.Array.Resize(ref outBuffer, siz);
        Marshal.Copy(ptr, outBuffer, 0, siz);

        sbuf.Free();
        releaseBuffer(ptr);

        return true;
    }



	// same as the decompressBuffer function. Only this one outputs to a buffer of fixed which size isn't resized to avoid memory allocations.
	// The fixed buffer should have a size that will be able to hold the incoming decompressed data.
	// Returns the uncompressed size.
	//
	// safe: if set to true the function will abort if the decompressed resut is larger the the fixed size output buffer.
	// Otherwise decompressed data will be written only until the end of the fixed output buffer.
	//
    public static int decompressBufferFixed(byte[] source, ref byte[] outBuffer, bool safe = true) {
        GCHandle sbuf = GCHandle.Alloc(source, GCHandleType.Pinned);
        IntPtr ptr;
        int siz = 0;

        ptr = zipDecompressBuffer(sbuf.AddrOfPinnedObject(), source.Length, ref siz);

        if (siz == 0 || ptr == IntPtr.Zero) { sbuf.Free(); releaseBuffer(ptr); return 0; }

		if (siz>outBuffer.Length) {
			if(safe) { sbuf.Free(); releaseBuffer(ptr); return 0; } else { siz = outBuffer.Length; }
		}

        Marshal.Copy(ptr, outBuffer, 0, siz);

        sbuf.Free();
        releaseBuffer(ptr);

        return siz;
    }



    // A function that decompresses a zlib compressed buffer and creates a new buffer.  Returns a new buffer with the uncompressed data.
    //
    // source                : a zlib compressed buffer.
    //
    // ERROR CODES           : a valid byte buffer = success
    //                       : null                = failed
    //
    public static byte[] decompressBuffer(byte[] source) {
        GCHandle sbuf = GCHandle.Alloc(source, GCHandleType.Pinned);
        IntPtr ptr;
        int siz = 0;

        ptr = zipDecompressBuffer(sbuf.AddrOfPinnedObject(), source.Length, ref siz);

        if (siz == 0 || ptr == IntPtr.Zero) { sbuf.Free(); releaseBuffer(ptr); return null; }

        byte[] buffer = new byte[siz];
        Marshal.Copy(ptr, buffer, 0, siz);

        sbuf.Free();
        releaseBuffer(ptr);

        return buffer;
    }



    // A function that will decompress a file in a zip archive directly in a provided byte buffer.
    //
    // zipArchive       : the full path to the zip archive from which a specific file will be extracted to a byte buffer.
    // entry            : the file we want to extract to a buffer. (If the file resides in a directory, the directory should be included.)
    // buffer           : a referenced byte buffer that will be resized and will be filled with the extraction data.
	// FileBuffer		: A buffer that holds a zip file. When assigned the function will read from this buffer and will ignore the filePath. 
	// password			: If the archive is encrypted use a password. (not available for WSA)
    //
    // ERROR CODES      :  1 = success
    //                  : -2 = could not find/open zip archive
	//					: -3 = could not locate entry
	//					: -4 = could not get entry info
	//					: -5 = password error
	//					: -18 = the entry has no size
	//					: -104 = internal memory error
    //
    public static int entry2Buffer(string zipArchive, string entry, ref byte[] buffer, byte[] FileBuffer = null, string password = null) {

		int siz;
		if(password == "") password = null;
		#if (UNITY_IPHONE || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_STANDALONE_LINUX || UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_WSA)
			if(FileBuffer!= null) {
				GCHandle fbuf = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);
				siz = zipGetEntrySize(null, entry, fbuf.AddrOfPinnedObject(), FileBuffer.Length);
				fbuf.Free();
			}else{
				siz = zipGetEntrySize( @zipArchive,  entry, IntPtr.Zero, 0);
			}
			#else
			siz = zipGetEntrySize( @zipArchive,  entry, IntPtr.Zero, 0);
		#endif

        if (siz <= 0) return -18;

        System.Array.Resize(ref buffer, siz);

        GCHandle sbuf = GCHandle.Alloc(buffer, GCHandleType.Pinned);

		int res;
		#if (UNITY_IPHONE || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_STANDALONE_LINUX || UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_WSA)
			if(FileBuffer!= null) {
				GCHandle fbuf = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);
				res = zipEntry2Buffer(null, entry, sbuf.AddrOfPinnedObject(), siz, fbuf.AddrOfPinnedObject(), FileBuffer.Length, password);
				fbuf.Free();
			}else{
				res = zipEntry2Buffer( @zipArchive, entry, sbuf.AddrOfPinnedObject(), siz, IntPtr.Zero, 0, password);
			}
			#else
			res = zipEntry2Buffer( @zipArchive, entry, sbuf.AddrOfPinnedObject(), siz, IntPtr.Zero, 0, password);
		#endif

        sbuf.Free();

        return res;
    }

    // A function that will decompress a file in a zip archive directly in a provided fixed size byte buffer.
    //
	// Returns the uncompressed size of the entry.
	//
    // zipArchive       : the full path to the zip archive from which a specific file will be extracted to a byte buffer.
    // entry            : the file we want to extract to a buffer. (If the file resides in a directory, the directory should be included.)
    // buffer           : a referenced fixed size byte buffer that will be filled with the extraction data. It should be large enough to store the data.
	// FileBuffer		: a buffer that holds a zip file. When assigned the function will read from this buffer and will ignore the filePath. 
	// password			: if the archive is encrypted use a password. (not available for WSA)
    //
    // ERROR CODES      :  1 = success
    //                  : -2 = could not find/open zip archive
	//					: -3 = could not locate entry
	//					: -4 = could not get entry info
	//					: -5 = password error
	//					: -18 = the entry has no size
	//					: -19 = the fixed size buffer is not big enough to store the uncompressed data
	//					: -104 = internal memory error
    //
    public static int entry2FixedBuffer(string zipArchive, string entry, ref byte[] fixedBuffer, byte[] FileBuffer = null, string password = null) {

		int siz;
		if(password == "") password = null;
		#if (UNITY_IPHONE || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_STANDALONE_LINUX || UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_WSA)
			if(FileBuffer!= null) {
				GCHandle fbuf = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);
				siz = zipGetEntrySize(null, entry, fbuf.AddrOfPinnedObject(), FileBuffer.Length);
				fbuf.Free();
			}else{
				siz = zipGetEntrySize( @zipArchive,  entry, IntPtr.Zero, 0);
			}
			#else
			siz = zipGetEntrySize( @zipArchive,  entry, IntPtr.Zero, 0);
		#endif

        if (siz <= 0) return -18;

		if(fixedBuffer.Length < siz) return -19;

        GCHandle sbuf = GCHandle.Alloc(fixedBuffer, GCHandleType.Pinned);

		int res;
		#if (UNITY_IPHONE || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_STANDALONE_LINUX || UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_WSA)
			if(FileBuffer!= null) {
				GCHandle fbuf = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);
				res = zipEntry2Buffer(null, entry, sbuf.AddrOfPinnedObject(), siz, fbuf.AddrOfPinnedObject(), FileBuffer.Length, password);
				fbuf.Free();
			}else{
				res = zipEntry2Buffer( @zipArchive, entry, sbuf.AddrOfPinnedObject(), siz, IntPtr.Zero, 0, password);
			}
			#else
			res = zipEntry2Buffer( @zipArchive, entry, sbuf.AddrOfPinnedObject(), siz, IntPtr.Zero, 0, password);
		#endif

        sbuf.Free();

        if(res != 1) return res;

		return siz;
    }


    // A function that will decompress a file in a zip archive in a new created and returned byte buffer.
    //
    // zipArchive       : the full path to the zip archive from which a specific file will be extracted to a byte buffer.
    // entry            : the file we want to extract to a buffer. (If the file resides in a directory, the directory should be included.
	// FileBuffer		: A buffer that holds a zip file. When assigned the function will read from this buffer and will ignore the filePath. 
	// password			: If the archive is encrypted use a password. (not available for WSA)
    //
    // ERROR CODES		: non-null  = success
    //                  : null      = failed
    //
    public static byte[] entry2Buffer(string zipArchive, string entry, byte[] FileBuffer = null, string password = null) {
		int siz;
		if(password == "") password = null;
		#if (UNITY_IPHONE || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_STANDALONE_LINUX || UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_WSA)
			if(FileBuffer!= null) {
				GCHandle fbuf = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);
				siz = zipGetEntrySize(null, entry, fbuf.AddrOfPinnedObject(), FileBuffer.Length);
				fbuf.Free();
			}else{
				siz = zipGetEntrySize( @zipArchive,  entry, IntPtr.Zero, 0);
			}
			#else
			siz = zipGetEntrySize( @zipArchive,  entry, IntPtr.Zero, 0);
		#endif

        if (siz <= 0) return null;

        byte[] buffer = new byte[siz];

        GCHandle sbuf = GCHandle.Alloc(buffer, GCHandleType.Pinned);

		int res;
		#if (UNITY_IPHONE || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_STANDALONE_LINUX || UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_WSA)
			if(FileBuffer!= null) {
				GCHandle fbuf = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);
				res = zipEntry2Buffer(null, entry, sbuf.AddrOfPinnedObject(), siz, fbuf.AddrOfPinnedObject(), FileBuffer.Length, password);
				fbuf.Free();
			}else{
				res = zipEntry2Buffer( @zipArchive, entry, sbuf.AddrOfPinnedObject(), siz, IntPtr.Zero, 0, password);
			}
			#else
			res = zipEntry2Buffer( @zipArchive, entry, sbuf.AddrOfPinnedObject(), siz, IntPtr.Zero, 0, password);
		#endif

        sbuf.Free();
        if (res!=1) return null;
        else return buffer;
    }




    // A function that compresses a byte buffer and writes it to a zip file. I you set the append flag to true, the output will get appended to an existing zip archive.
    //
    // levelOfCompression   : (0-9) recommended 9 for maximum.
    // zipArchive           : the full path to the zip archive to be created or append to.
    // arc_filename         : the name of the file that will be written to the archive.
    // buffer               : the buffer that will be compressed and will be put in the zip archive.
    // append               : set to true if you want the output to be appended to an existing zip archive.
	// comment				: an optional comment for this entry.
	// password				: an optional password to encrypt this entry. (not available for WSA)
	// useBz2				: set to true if you want bz2 compression instead of zlib. (not available for MacOS/iOS/tvOS/watchOS)
    //
    // ERROR CODES          : true  = success
    //                      : false = failed
    //
    public static bool buffer2File(int levelOfCompression, string zipArchive, string arc_filename, byte[] buffer, bool append=false, string comment=null, string password=null
	#if !UNITY_EDITOR_OSX && !UNITY_STANDALONE_OSX && !UNITY_IOS && !UNITY_TVOS
	, bool useBz2 = false
	#endif
	) {
        if (!append) { if (File.Exists(@zipArchive)) File.Delete(@zipArchive); }
        GCHandle sbuf = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        if (levelOfCompression < 0) levelOfCompression = 0; if (levelOfCompression > 9) levelOfCompression = 9;
        if(password == "") password = null;
		if(comment == "") comment = null;
        bool res = zipBuf2File(levelOfCompression, @zipArchive, arc_filename, sbuf.AddrOfPinnedObject(), buffer.Length, comment, password
		#if !UNITY_EDITOR_OSX && !UNITY_STANDALONE_OSX && !UNITY_IOS && !UNITY_TVOS
		, useBz2
		#endif
		);

        sbuf.Free();
        return res;
    }




	// A function that deletes a file in a zip archive. It creates a temp file where the compressed data of the old archive is copied except the one that needs to be deleted.
	// After that the old zip archive is deleted and the temp file gets renamed to the original zip archive.
	// You can delete directories too if they are empty.
	//
	// zipArchive           : the full path to the zip archive
	// arc_filename         : the name of the file that will be deleted.
	//
	// ERROR CODES			:  1 = success
	//						: -1 = failed to open zip
	//						: -2 = failed to locate the archive to be deleted in the zip file
	//						: -3 = error copying compressed data from original zip
	//						: -4 = failed to create temp zip file.
	//
	public static int delete_entry(string zipArchive, string arc_filename) {
		string tmp = zipArchive+".tmp";
		int res = zipDeleteFile(zipArchive, arc_filename, tmp);

		
		if(res>0) {
			File.Delete(@zipArchive);
			#if UNITY_WSA && !UNITY_EDITOR
				fileRename(@tmp, @zipArchive);
			#else
				File.Move(@tmp, @zipArchive);
			#endif
		}else {
			if(File.Exists(@tmp)) File.Delete(@tmp);
		}

		return res;
	}



	// A function that replaces an entry in a zip archive with a file that lies in a path. The original name of the archive will be used.
	//
	// zipArchive           : the full path to the zip archive
	// arc_filename         : the name of the file that will be replaced.
	// newFilePath			: a path to the file that will replace the original entry.
	// level:				: the level of compression of the new entry.
    // comment				: add a comment for the file in the zip file header.
	// password				: set the password to protect this file. (only ascii characters) (not available for WSA)
	// useBz2				: use the bz2 compression algorithm. If false the zlib deflate algorithm will be used. (not available for MacOS/iOS/tvOS/watchOS)
	//
    // ERROR CODES
    //						:  -1 = could not create or append
    //						:  -2 = error during operation
    //						:  -3 = failed to delete original entry
	//

	public static int replace_entry(string zipArchive, string arc_filename, string newFilePath, int level = 9, string comment=null, string password = null
	#if !UNITY_EDITOR_OSX && !UNITY_STANDALONE_OSX && !UNITY_IOS && !UNITY_TVOS
	, bool useBz2=false
	#endif
	) {
		int res = delete_entry(zipArchive, arc_filename);
		if(res<0) return -3;
		if(password == "") password = null;
		if(comment == "") comment = null;
		return zipCD(level, zipArchive, newFilePath, arc_filename, comment, password
		#if !UNITY_EDITOR_OSX && !UNITY_STANDALONE_OSX && !UNITY_IOS && !UNITY_TVOS
		, useBz2
		#endif
		);
	}



	// A function that replaces an entry in a zip archive with a buffer. The original name of the archive will be used.
	//
	// zipArchive           : the full path to the zip archive
	// arc_filename         : the name of the file that will be replaced.
	// newFileBuffer		: a byte buffer that will replace the original entry.
	// level:				: the level of compression of the new entry.
	// password				: set the password to protect this file. (only ascii characters) (not available for WSA)
	// useBz2				: use the bz2 compression algorithm. If false the zlib deflate algorithm will be used. (not available for MacOS/iOS/tvOS/watchOS)
	//
    // ERROR CODES
	//                    :  1 = success
	//					  : -5 = failed to delete the original file
	//					  : -6 = failed to append the buffer to the zip

	public static int replace_entry(string zipArchive, string arc_filename, byte[] newFileBuffer, int level = 9, string password = null
	#if !UNITY_EDITOR_OSX && !UNITY_STANDALONE_OSX && !UNITY_IOS && !UNITY_TVOS
	, bool useBz2 = false
	#endif
	) {
		int res = delete_entry(zipArchive, arc_filename);
		if(res<0) return -5;

		if( buffer2File(level, zipArchive, arc_filename, newFileBuffer, true, null, password
		#if !UNITY_EDITOR_OSX && !UNITY_STANDALONE_OSX && !UNITY_IOS && !UNITY_TVOS
		, useBz2
		#endif
		) ) return 1; else return - 6;
	}



    // A function that will extract only the specified file that resides in a zip archive.
    //
    // zipArchive       : the full path to the zip archive from which we want to extract the specific file.
    // arc_filename     : the specific file we want to extract. (If the file resides in a  directory, the directory path should be included. like dir1/dir2/myfile.bin)
	//					:  --> on some zip files the internal dir structure uses \\ instead of / characters for directories separators. In that case use the appropriate
	//					:  --> chars that will allow the file to be extracted.
    // outpath          : the full path to where the file should be extracted + the desired name for it.
	// FileBuffer		: A buffer that holds a zip file. When assigned the function will read from this buffer and will ignore the filePath. 
	// proc:			: a single item integer array that gets updated with the progress of the decompression in bytes.
	//					  (100% is reached when the compressed size of the file is reached.)
	// password			: if needed, the password to decrypt the entry. (not available for WSA)
    //
    // ERROR CODES      : -1 = extraction failed
    //                  : -2 = could not initialize zip archive.
	//					: -3 = could not locate entry
	//					: -4 = could not get entry info
	//					: -5 = password error
    //                  :  1 = success
    //
    public static int extract_entry(string zipArchive, string arc_filename, string outpath, byte[] FileBuffer = null, int[] proc = null, string password = null) {

		if(!Directory.Exists( Path.GetDirectoryName(outpath) ) ) return -1;

		int res=-1;
		if(proc == null) proc = new int[1];
		GCHandle pbuf = GCHandle.Alloc(proc, GCHandleType.Pinned);

			#if (UNITY_IPHONE || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_STANDALONE_LINUX || UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_WSA)
			if(FileBuffer!= null) {
				GCHandle fbuf = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);

				if(proc != null) res = zipEntry(null, arc_filename, outpath, fbuf.AddrOfPinnedObject(), FileBuffer.Length, pbuf.AddrOfPinnedObject(), password);
				else res = zipEntry(null, arc_filename, outpath, fbuf.AddrOfPinnedObject(), FileBuffer.Length, IntPtr.Zero, password);

				fbuf.Free();
				pbuf.Free();
				return res;
			}
			#endif

		if(proc != null) res = zipEntry(@zipArchive, arc_filename, outpath, IntPtr.Zero, 0, pbuf.AddrOfPinnedObject(), password);
		else res = zipEntry(@zipArchive, arc_filename, outpath, IntPtr.Zero, 0, IntPtr.Zero, password);
		pbuf.Free();
		return res;
			
    }



    // A function that decompresses a zip file. If the zip contains directories, they will be created.
    //
    // zipArchive       : the full path to the zip archive that will be decompressed.
    // outPath          : the directory in which the zip contents will be extracted.
    // progress         : provide a single item integer array to write the current index of the file getting extracted. To use it in realtime, call
    //                  : this function in a separate thread.
	// FileBuffer		: A buffer that holds a zip file. When assigned the function will read from this buffer and will ignore the filePath. 
	// proc:			: a single item integer array that gets updated with the progress of the decompression in bytes.
	//					  (100% is reached when the compressed size of the file is reached.)
	// password			: if needed, the password to decrypt the archive. (not available for WSA)
    //
    // ERROR CODES
    //                  : -1 = could not initialize zip archive.
    //                  : -2 = failed extraction
    //                  :  1 = success
    //
    public static int decompress_File(string zipArchive, string outPath, int[] progress,  byte[] FileBuffer = null, int[] proc = null, string password = null) {
        //make a check if the last '/' exists at the end of the exctractionPath and add it if it is missing
        if (@outPath.Substring(@outPath.Length - 1, 1) != "/") { @outPath += "/"; }
		int res;
		GCHandle ibuf = GCHandle.Alloc(progress, GCHandleType.Pinned);
		if(proc == null) proc = new int[1];
		GCHandle pbuf = GCHandle.Alloc(proc, GCHandleType.Pinned);
		
			#if (UNITY_IPHONE || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_STANDALONE_LINUX || UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_WSA)
			if(FileBuffer!= null) {
				GCHandle fbuf = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);

				if(proc != null) res = zipEX(null, @outPath, ibuf.AddrOfPinnedObject(), fbuf.AddrOfPinnedObject(), FileBuffer.Length, pbuf.AddrOfPinnedObject(), password);
				else res = zipEX(null, @outPath, ibuf.AddrOfPinnedObject(), fbuf.AddrOfPinnedObject(), FileBuffer.Length, IntPtr.Zero, password);

				fbuf.Free(); ibuf.Free(); pbuf.Free(); return res;
			}
			#endif

		if(proc != null) res = zipEX(@zipArchive, @outPath, ibuf.AddrOfPinnedObject(), IntPtr.Zero, 0, pbuf.AddrOfPinnedObject(), password);
		else res = zipEX(@zipArchive, @outPath, ibuf.AddrOfPinnedObject(), IntPtr.Zero, 0, IntPtr.Zero, password);
		ibuf.Free(); pbuf.Free();
		return res;
    }




    // A function that compresses a file to a zip file. If the flag append is set to true then it will get appended to an existing zip file.
    //
    // levelOfCompression : (0-9) recommended 9 for maximum (10 is highest but slower and not zlib compatible)
    // zipArchive         : the full path to the zip archive that will be created
    // inFilePath         : the full path to the file that should be compressed and added to the zip file.
    // append             : set to true if you want the input file to get appended to an existing zip file. (if the zip file does not exist it will be created.)
    // filename           : if you want the name of your file to be different then the one it has, set it here. If you add a folder structure to it,
    //                      like (dir1/dir2/myfile.bin) the directories will be created in the zip file.
    // comment            : add a comment for the file in the zip file header.
	// password			  : set the password to protect this file. (only ascii characters) (not available for WSA)
	// useBz2			  : use the bz2 compression algorithm. If false the zlib deflate algorithm will be used. (not available for MacOS/iOS/tvOS/watchOS)
    //
    // ERROR CODES
	//					  :   1 = success
    //                    :  -1 = could not create or append
    //                    :  -2 = error during operation
	//
    public static int compress_File(int levelOfCompression, string zipArchive, string inFilePath,bool append=false, string fileName="", string comment=null, string password = null
	#if !UNITY_EDITOR_OSX && !UNITY_STANDALONE_OSX && !UNITY_IOS && !UNITY_TVOS
	, bool useBz2 = false
	#endif
	) {
		if (!File.Exists(@inFilePath)) return -10;
        if (!append) { if (File.Exists(@zipArchive)) File.Delete(@zipArchive); }

        if (fileName == "") fileName = Path.GetFileName(@inFilePath);
        if (levelOfCompression < 0) levelOfCompression = 0; if (levelOfCompression > 9) levelOfCompression = 9;
        if(password == "") password = null;
		if(comment == "") comment = null;

        return zipCD(levelOfCompression, @zipArchive, @inFilePath, fileName, comment, password
		#if !UNITY_EDITOR_OSX && !UNITY_STANDALONE_OSX && !UNITY_IOS && !UNITY_TVOS
		, useBz2
		#endif
		);
    }


    // A function that compresses a list of files to a zip file.
    //
    // levelOfCompression : (0-9) recommended 9 for maximum (10 is highest but slower and not zlib compatible)
    // zipArchive         : the full path to the zip archive that will be created
    // inFilePath[]       : an array of the full paths to the files that should be compressed and added to the zip file.
	// progress			  : this var will increment until the number of the input files and this are equal.
    // append             : set to true if you want the input file to get appended to an existing zip file. (if the zip file does not exist it will be created.)
    // filename[]         : if you want the names of your files to be different then the one they have, set it here. If you add a folder structure to it,
    //                      like (dir1/dir2/myfile.bin) the directories will be created in the zip file.
	// password			  : set the password to protect this file. (only ascii characters) (not available for WSA)
	// useBz2			  : use the bz2 compression algorithm. If false the zlib deflate algorithm will be used. (not available for MacOS/iOS/tvOS/watchOS)
    //
    // ERROR CODES
	//					  :   1 = success
    //                    :  -1 = could not create or append
    //                    :  -2 = error during operation
	//
    public static int compress_File_List(int levelOfCompression, string zipArchive, string[] inFilePath, int[] progress = null, bool append=false, string[] fileName=null, string password = null
	#if !UNITY_EDITOR_OSX && !UNITY_STANDALONE_OSX && !UNITY_IOS && !UNITY_TVOS
	, bool useBz2 = false
	#endif
	) {

		if (inFilePath == null) return -3;

		if(fileName!=null && fileName.Length != inFilePath.Length) return -4;

		for(int i=0; i<inFilePath.Length; i++) {
			if (!File.Exists(@inFilePath[i])) return -10;
		}

        if (!append) { if (File.Exists(@zipArchive)) File.Delete(@zipArchive); }

        if (levelOfCompression < 0) levelOfCompression = 0; if (levelOfCompression > 9) levelOfCompression = 9;
        if(password == "") password = null;

		int res = 0;

		string[] fna = null;
		string path = Path.GetDirectoryName(zipArchive);

		if(fileName == null) {
			fna = new string[inFilePath.Length];
			for(int i=0; i<inFilePath.Length; i++) { fna[i] = inFilePath[i].Replace(path, "");  }
		} else {
			fna = fileName;
		}

		for(int i=0; i<inFilePath.Length; i++) { if(fna[i] == null) fna[i] = inFilePath[i].Replace(path, "");   }

		for(int i=0;i<inFilePath.Length; i++) {
			if(progress != null) progress[0]++;
			res = compress_File(levelOfCompression, @zipArchive, inFilePath[i], true, fna[i], null, password
			#if !UNITY_EDITOR_OSX && !UNITY_STANDALONE_OSX && !UNITY_IOS && !UNITY_TVOS
			, useBz2
			#endif
			);
		}

		path = null;

		return res;
    }

    //integer that stores the current compressed number of files
    public static int cProgress = 0;

    // Compress a directory with all its files and subfolders to a zip file (Does not work on WSA 8.1)
    //
    // sourceDir           : the directory you want to compress
    // levelOfCompression  : the level of compression (0-9).
    // zipArchive          : the full path+name to the zip file to be created .
    // includeRoot         : set to true if you want the root folder of the directory to be included in the zip archive. Otherwise leave it to false.
	// password			   : set the password to protect this file. (only ascii characters) (not available for WSA)
	// useBz2			   : use the bz2 compression algorithm. If false the zlib deflate algorithm will be used. (not available for MacOS/iOS/tvOS/watchOS)
    //
    // If you want to get the progress of compression, call the getAllFiles function to get the total number of files
    // in a directory and its subdirectories. The compressDir when called from a separate thread will update the public static int cProgress.
    // Divide this with the total number of files (as floats) and you have the % of the procedure.
	//
	#if !(UNITY_WSA_8_1 || UNITY_WP_8_1 || UNITY_WINRT_8_1) || UNITY_EDITOR
		public static void compressDir(string sourceDir, int levelOfCompression, string zipArchive, bool includeRoot = false, string password = null
		#if !UNITY_EDITOR_OSX && !UNITY_STANDALONE_OSX && !UNITY_IOS && !UNITY_TVOS
		, bool useBz2 = false
		#endif
		) {
			string fdir = @sourceDir.Replace("\\", "/");

			#if (UNITY_WSA) && !UNITY_EDITOR
			if(UnityEngine.Windows.Directory.Exists(fdir)) {
			#else
			if(Directory.Exists(fdir)) {
			#endif
				if (File.Exists(@zipArchive)) File.Delete(@zipArchive);
				string[] ss = fdir.Split('/');
				string rdir = ss[ss.Length - 1];
				string root = rdir;
				
				cProgress = 0;

				if (levelOfCompression < 0) levelOfCompression = 0; if (levelOfCompression > 9) levelOfCompression = 9;

				foreach (string f in Directory.GetFiles(fdir, "*", SearchOption.AllDirectories)){
					string s = f.Replace(fdir, rdir).Replace("\\", "/");

					if (!includeRoot) s = s.Substring(root.Length+1);

					compress_File(levelOfCompression, @zipArchive, f, true, s, null, password
					#if !UNITY_EDITOR_OSX && !UNITY_STANDALONE_OSX && !UNITY_IOS && !UNITY_TVOS
					, useBz2
					#endif
					);
					cProgress++;
				}
			} 
		}

	
		//Use this function to get the total files of a directory and its subdirectories. (Does not work on WSA 8.1)
		public static int getAllFiles(string Dir) {
			string[] filePaths = Directory.GetFiles(@Dir, "*", SearchOption.AllDirectories);
			int res = filePaths.Length;
			filePaths = null;
			return res;
		}
	#endif

#endif


	//---------------------------------------------------------------------------------------------------------------------------
	//GZIP SECTION
	//---------------------------------------------------------------------------------------------------------------------------

	// compress a byte buffer to gzip format.
	//
	// returns the size of the compressed buffer.
	//
	// source:		the uncompressed input buffer.
	// outBuffer:	the provided output buffer where the compressed data will be stored (it should be at least the size of the input buffer +18 bytes).
	// level:		the level of compression (0-9)
	// addHeader:	if a gzip header should be added. (recommended if you want to write out a gzip file)
	// addFooter:	if a gzip footer should be added. (recommended if you want to write out a gzip file)
	//
	public static int gzip(byte[] source, byte[] outBuffer, int level, bool addHeader = true, bool addFooter = true) {

		GCHandle sbuf = GCHandle.Alloc(source, GCHandleType.Pinned);
		GCHandle dbuf = GCHandle.Alloc(outBuffer, GCHandleType.Pinned);

		int res = zipGzip(sbuf.AddrOfPinnedObject(), source.Length, dbuf.AddrOfPinnedObject(), level, addHeader, addFooter);

		sbuf.Free(); dbuf.Free();
		int hf = 0;
		if(addHeader) hf += 10;
		if(addFooter) hf += 8;
		return res + hf;
	}


	// get the uncompressed size from a gzip buffer that has a footer included
	//
	// source:		the gzip compressed input buffer. (it should have at least a gzip footer)
	public static int gzipUncompressedSize(byte[] source) {
		int res = source.Length;
		uint size = ((uint)source[res-4] & 0xff) |
				((uint)source[res-3] & 0xff) << 8 |
				((uint)source[res-2] & 0xff) << 16 |
				((uint)source[res-1]& 0xff) << 24;
		return (int)size;
	}

	// decompress a gzip buffer
	//
	// returns:		uncompressed size. negative error code on error. 
	//
	// source:		the gzip compressed input buffer.
	// outBuffer:	the provided output buffer where the uncompressed data will be stored.
	// hasHeader:	if the buffer has a header.
	// hasFooter:	if the buffer has a footer.
	//
	public static int unGzip(byte[] source, byte[] outBuffer, bool hasHeader = true, bool hasFooter = true) {
		GCHandle sbuf = GCHandle.Alloc(source, GCHandleType.Pinned);
		GCHandle dbuf = GCHandle.Alloc(outBuffer, GCHandleType.Pinned);
		
		int res = zipUnGzip( sbuf.AddrOfPinnedObject(), source.Length, dbuf.AddrOfPinnedObject(),outBuffer.Length, hasHeader, hasFooter );

		sbuf.Free(); dbuf.Free();
		return res;				
	}

	// decompress a gzip buffer (This function assumes that the gzip buffer has a gzip header !!!)
	//
	// returns:		uncompressed size. negative error code on error. 
	//
	// source:		the gzip compressed input buffer.
	// outBuffer:	the provided output buffer where the uncompressed data will be stored.
	//
	public static int unGzip2(byte[] source, byte[] outBuffer) {
		GCHandle sbuf = GCHandle.Alloc(source, GCHandleType.Pinned);
		GCHandle dbuf = GCHandle.Alloc(outBuffer, GCHandleType.Pinned);

		int res = zipUnGzip2( sbuf.AddrOfPinnedObject(), source.Length, dbuf.AddrOfPinnedObject(), outBuffer.Length );

		sbuf.Free(); dbuf.Free();
		return res;				
	}


	// get the DateTime of an entry in a zip archive
	//
    // zipArchive       : the full path to the zip archive from which we want to extract the specific file.
    // entry		    : the specific entry we want to get the DateTime of. (If the entry resides in a  directory, the directory path should be included. like dir1/dir2/myfile.bin)
	// FileBuffer		: A buffer that holds a zip file. When assigned the function will read from this buffer and will ignore the zipArchive path.
	//
	// Returns the date and time of the entry in DateTime format.
	//
	// ERROR CODES
	//					: 0 = Cannot open zip Archive
	//					: 1 = entry not found
	//					: 2 = error reading entry
	//
	public static DateTime entryDateTime(string zipArchive, string entry, byte[] FileBuffer = null) {

		uint dosDateTime = 0;
		
		#if (UNITY_IPHONE || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_STANDALONE_LINUX || UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_WSA)
		if(FileBuffer!= null) {
			GCHandle fbuf = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);
			dosDateTime = getEntryDateTime(null, entry,  fbuf.AddrOfPinnedObject(), FileBuffer.Length);
			fbuf.Free();
		}
		#endif
		
		if(FileBuffer == null) dosDateTime = getEntryDateTime(zipArchive, entry,  IntPtr.Zero, 0);
		
		var date = (dosDateTime & 0xFFFF0000) >> 16;
		var time = (dosDateTime & 0x0000FFFF);

		var year = (date >> 9) + 1980;
		var month = (date & 0x01e0) >> 5;
		var day =  date & 0x1F;
		var hour = time >> 11;
		var minute = (time & 0x07e0) >> 5;
		var second = (time & 0x1F) * 2;

		if(dosDateTime == 0 || dosDateTime == 1 || dosDateTime == 2) { Debug.Log("Error in getting DateTime"); return DateTime.Now; }

		return new DateTime((int)year, (int)month, (int)day, (int)hour, (int)minute, (int)second);
	}

#endif
}
