namespace TexPup
{
    struct ProgressReport
    {
        public float Value;

        public string Status;

        public ProgressReport(float value, string status)
        {
            Value = value;
            Status = status;
        }
    }
}
