using AmphetamineSerializer;
using AmphetamineSerializer.Example;
using System.Collections.Generic;
using System.IO;

namespace Mermec.Trackware.NativeTestSuite
{
    public class PeParser
    {
        public static void Parse(string path = @"C: \Users\Marco\Desktop\devenv.exe")
        {
            var dosHeader                   = new DosHeader();
            var ntHeader                    = new NtHeader();
            var sections                    = new List<ImageSectionHeader>(4);
            var importDirectory             = new ImportDirectory();
            
            var dosHeaderSerializator       = new TWSerializator<DosHeader>();
            var ntHeaderSerializator        = new TWSerializator<NtHeader>();
            var sectionHeaderSerializator   = new TWSerializator<ImageSectionHeader>();
            var importDirectorySerializator = new TWSerializator<ImportDirectory>();

            using (var file = File.Open(path, FileMode.Open))
            {
                BinaryReader reader = new BinaryReader(file);
                dosHeaderSerializator.Deserialize(ref dosHeader, reader);
                reader.BaseStream.Position = dosHeader.e_lfanew;
                ntHeaderSerializator.Deserialize(ref ntHeader, reader);

                for (int i = 0; i < ntHeader.FileHeader.NumberOfSections; ++i)
                {
                    var currentSection = new ImageSectionHeader();
                    sectionHeaderSerializator.Deserialize(ref currentSection, reader);
                    sections.Add(currentSection);
                }
                uint offset = VAToFileOffset(sections, ntHeader.OptionalHeader.DataDirectory[1].VirtualAddress);

                reader.BaseStream.Position = offset;
                importDirectorySerializator.Deserialize(ref importDirectory, reader);
            }
        }

        private static uint VAToFileOffset(List<ImageSectionHeader> sections, uint virtualAddress)
        {
            foreach (var item in sections)
            {
                uint minVA = item.VirtualAddress;
                uint maxVA = item.Misc + minVA;
                if (virtualAddress > minVA && virtualAddress < maxVA)
                {
                    virtualAddress -= minVA;
                    virtualAddress += item.PointerToRawData;
                    return virtualAddress;
                }
            }
            return 0;
        }
    }
}