namespace InspectFileUsingPeCoff
{
    internal class FileInspectionResponse
    {
        public BitnessType Bitness { get; set; }

        public bool HasComServerExports { get; set; }

        public bool IsManaged { get; set; }

        public bool IsStrongNameSigned { get; set; }
    }
}
