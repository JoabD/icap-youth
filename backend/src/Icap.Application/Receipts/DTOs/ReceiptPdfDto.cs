namespace Icap.Application.Receipts.DTOs;

public sealed record ReceiptPdfDto(byte[] Content, string FileName);
