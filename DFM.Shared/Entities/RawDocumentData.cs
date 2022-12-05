using DFM.Shared.Common;
using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Entities
{
    public class RawDocumentData
    {
        [Indexed]
        public string? DataID { get; set; } //= Guid.NewGuid().ToString("N");// ແມ່ນ Unique ID ທີ່ເອົາໄວ້ Map ກັບຂໍ້ມູນຜູ້ຮັບ ທີ່ຢູ່ໃນ Field Reciepients
        [Indexed]
        public string? Title { get; set; }
        [Indexed]
        public string? ExternalDocID { get; set; }
        [Indexed]
        public string? DocDate { get; set; }
        [Indexed]
        public string? ResponseUnit { get; set; }
        [Indexed]
        public string? FromUnit { get; set; }
        [Indexed]
        public int DocPage { get; set; }
        [Indexed(CascadeDepth = 1)]
        public DocumentSecurityModel Security { get; set; } = new();
        [Indexed(CascadeDepth = 1)]
        public DocumentUrgentModel Urgent { get; set; } = new();
        [Indexed]
        public string? FormType { get; set; }
        [Indexed]
        public string? TransferType { get; set; }
        [Indexed]
        public bool IsOriginalFile { get; set; }

        [Indexed]
        public string? IncomingDate { get; set; }

        [Indexed]
        public string? ExpiredDate { get; set; }

        [Indexed]
        public string? SetDate { get; set; }
        [Indexed]
        public string? CommentNo { get; set; }
        [Indexed]
        public string? CommentTitle { get; set; }
        [Indexed]
        public string? CommentDetail { get; set; }
        [Indexed(CascadeDepth = 1)]
        public List<AttachmentModel> Attachments { get; set; } = new();
        [Indexed(CascadeDepth = 1)]
        public List<AttachmentModel> RelateFles { get; set; } = new();
        [Indexed]
        public string? CopyType { get; set; }
        [Indexed]
        public string? ExternalUnit { get; set; }

        [Indexed]
        public string? SendDate { get; set; }
        [Indexed]
        public string? Signer { get; set; }

        [Indexed]
        public string? FolderId { get; set; }
        [Indexed]
        public string? DocType { get; set; }
        [Indexed]
        public string? DocNo { get; set; }
        [Indexed]
        public int FolderNum { get; set; }// ແມ່ນເລກ ຂອງ Folder
    }
}
