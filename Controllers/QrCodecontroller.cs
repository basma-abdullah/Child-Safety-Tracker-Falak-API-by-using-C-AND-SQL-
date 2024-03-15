/*using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using QRCoder;


namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QrCodeController : ControllerBase
    {
        private readonly string connectionString;  // Connection string to your database

        public QrCodeController()
        {
            // Initialize the connection string
            connectionString = "YourConnectionString";
        }

        [HttpGet("generateQrCode/{userId}/{childId}")]
        public IActionResult GenerateQrCode(int userId, int childId)
        {
            // Retrieve child information from the database based on the childId and userId
            var child = GetChildInformation(childId, userId);
            if (child == null)
            {
                return NotFound("Child not found");
            }

            // Retrieve parent information from the database based on the userId
            var parent = GetParentInformation(userId);
            if (parent == null)
            {
                return NotFound("Parent not found");
            }

            // Generate the content of the QR code
            string qrCodeContent = $"Child Name: {child.FullName}\n" +
                                   $"Age: {DateTime.Now.Year - child.YearOfBirth}\n" +
                                   $"Gender: {child.Gender}\n" +
                                   $"Additional Information: {child.AdditionalInformation}\n" +
                                   $"Parent Name: {parent.FullName}\n" +
                                   $"Parent Phone: {parent.PhoneNumber}";

            // Create a QR code generator instance
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeContent, QRCodeGenerator.ECCLevel.Q);
           
            // Create a QR code instance
            QRCode qrCode = new QRCode(qrCodeData);

            // Convert the QR code to a bitmap image
            Bitmap qrCodeImage = qrCode.GetGraphic(pixelsPerModule: 10);

            // Save the QR code image to a memory stream
            MemoryStream memoryStream = new MemoryStream();
            qrCodeImage.Save(memoryStream, ImageFormat.Png);
            memoryStream.Position = 0;

            // Return the QR code image as a file response
            return File(memoryStream, "image/png", "qr_code.png");
        }

        private ChildInformation GetChildInformation(int childId, int userId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = $"SELECT c.FullName, c.YearOfBirth, c.Gender, c.AdditionalInformation " +
                               $"FROM personChild c " +
                               $"WHERE c.childID = @childId AND c.MainPersonInChargeID = @userId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@childId", childId);
                    command.Parameters.AddWithValue("@userId", userId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Retrieve the child information from the reader
                            var fullName = reader.GetString(reader.GetOrdinal("FullName"));
                            var yearOfBirth = reader.GetInt32(reader.GetOrdinal("YearOfBirth"));
                            var gender = reader.GetString(reader.GetOrdinal("Gender"));
                            var additionalInformation = reader.GetString(reader.GetOrdinal("AdditionalInformation"));

                            return new ChildInformation
                            {
                                FullName = fullName,
                                YearOfBirth = yearOfBirth,
                                Gender = gender,
                                AdditionalInformation = additionalInformation
                            };
                        }
                    }
                }
            }

            return null;
        }

        private ParentInformation GetParentInformation(int userId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = $"SELECT u.FullName, u.PhoneNumber " +
                               $"FROM personUsers u " +
                               $"WHERE u.UserID = @userId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Retrieve the parent information from the reader
                            var fullName = reader.GetString(reader.GetOrdinal("FullName"));
                            var phoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber"));

                            return new ParentInformation
                            {
                                FullName = fullName,
                                PhoneNumber = phoneNumber
                            };
                        }
                    }
                }
            }

            return null;
        }
    }

    public class ChildInformation
    {
        public string FullName { get; set; }
        public int YearOfBirth { get; set; }
        public string Gender { get; set; }
        public string AdditionalInformation { get; set; }
    }

    public class ParentInformation
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
    }
}
*/