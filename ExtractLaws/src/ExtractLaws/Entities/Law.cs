using System;
using System.Collections.Generic;

namespace ExtractLaws.Entities
{
    public class Law
    {
        public string LawName { get; set; }
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime LawDate { get; set; }
        public DateTime DigitalVersionDate { get; set; }
        public string Body { get; set; }
        public string Publication { get; set; }
        public string Kind { get; set; }
        public string Valid { get; set; }

        public Law(string lawName,DateTime lawDate, string body, string kind, DateTime digitalVersionDate, string publication, string valid)
        {
            LawName = lawName;
            LawDate = lawDate;
            Body = body;
            Kind = kind;
            DigitalVersionDate = digitalVersionDate;
            Publication = publication;
            Valid = valid;
        }
    }
}