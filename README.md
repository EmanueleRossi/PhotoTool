# PhotoTool
CMD line tool for EXIF Photo Management.
Useful when migrating from iCloud to an other Photo Storage/Management system.
This tool is based on ExifTool by Phil Harvey [https://exiftool.org/](https://exiftool.org/).

# Pre-Requisites
The tool itself <code>exiftool.exe</code> must be somewhere in <code>PATH</code>.

# Usage Sample
<code>PhotoTool.exe c:\Images *.xmp</code>

Analizes all XMP files in images directory.
For every XMP file:
1. reads inside XMP file tag <code>photoshop:DateCreated</code> in namespace <code>http://ns.adobe.com/photoshop/1.0/</code>
2. looks for image file(s) (supported formats are <code>jpg, jpeg, mp4, mov, heic</code>) with the same name of the XMP file. I.e.: <code>image_01.xmp -> image_01.jpeg</code>
3. reads file(s) EXIF informations, looking for TAG <code>DateTimeOriginal</code>;
4. if no <code>DateTimeOriginal</code> TAG is found, overwrites it with what found at Step #01;
5. renames XMP file to XMP.done, letting you to re-process the entire directory;

# Snippets
Count DONE objects using PowerShell:
<code>dir C:\Images *.xmp.done | Measure-Object -line</code>