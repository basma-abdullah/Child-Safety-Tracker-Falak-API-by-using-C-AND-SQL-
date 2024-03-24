using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using QRCoder;


namespace QRCodes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QrCodeController : ControllerBase
    {
        private readonly string connectionString;  // Connection string to your database

        public QrCodeController()
        {
            // Initialize the connection string
            connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Admin\\OneDrive\\FalakDB.mdf;Integrated Security=True;Connect Timeout=30";
        }

        //this will generate qrcode information to use in child card
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
                                   $"Parent Phone: {parent.PhoneNumber.ToString()}";

            // Create a QR code generator instance
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeContent, QRCodeGenerator.ECCLevel.Q);

            // Create a QR code instance
            QRCode qrCode = new QRCode(qrCodeData);


            // Convert the QR code to a bitmap image
            Bitmap qrCodeImage = qrCode.GetGraphic(pixelsPerModule: 10);




            // Save the QR code image to a memory stream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                //qrCodeImage.Save(memoryStream, ImageFormat.Png);
                //memoryStream.Position = 0;

                // Convert the QR code image to a base64 string
                string base64Image = ConvertImageToBase64(qrCodeImage);


                // Store the QR code image in the database
                StoreQrCodeInDatabase(childId, base64Image);
            }

            // Return a success response
            return Ok("QR code generated and stored in the database.");
            // Save the QR code image to a memory stream
            //MemoryStream memoryStream = new MemoryStream();
            //qrCodeImage.Save(memoryStream, ImageFormat.Png);
            //memoryStream.Position = 0;

            // Return the QR code image as a file response
            //return File(memoryStream, "image/png", "qr_code.png");
        }

        private ChildInformation GetChildInformation(int childId, int userId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = $"SELECT cn.FullName, c.YearOfBirth, cn.Gender, c.AdditionalInformation " +
                               $"FROM PersonUsers p, PersonUsers cn, PersonChilds c " +
                               $"WHERE c.ChildID = cn.UserID AND c.ChildID = @childId AND c.MainPersonInChargeID = @userId AND p.userid = c.MainPersonInChargeID";
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
                               $"FROM PersonUsers u " +
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
                            var phoneNumber = reader.GetInt32(reader.GetOrdinal("PhoneNumber"));

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

        private void StoreQrCodeInDatabase(int childId, string qrCode)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = $"UPDATE PersonChilds SET QRCodeInfo = @qrCode WHERE ChildID = @childId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@childId", childId);
                    command.Parameters.AddWithValue("@qrCode", qrCode);

                    command.ExecuteNonQuery();
                }
            }
        }

        [HttpGet("getQrCode/{childId}")]
        public IActionResult GetQrCode(int childId)
        {
            // Retrieve the QR code from the database based on the childId
            string qrCodeString = GetQrCodeFromDatabase(childId);
            if (string.IsNullOrEmpty(qrCodeString))
            {
                return NotFound("QR code not found");
            }

            // Convert the QR code string to a byte array
            byte[] qrCodeBytes = Convert.FromBase64String(qrCodeString);

            // Return the byte array as an image file
            return File(qrCodeBytes, "image/png");
        }

        private string GetQrCodeFromDatabase(int childId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = $"SELECT QRCodeInfo FROM PersonChilds WHERE ChildID = @childId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@childId", childId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Check if the QRCode column is not null or empty
                            if (!reader.IsDBNull(0))
                            {
                                return reader.GetString(0);
                            }
                        }
                    }
                }
            }

            return null;
        }

        private Bitmap ConvertStringToImage(string qrCodeString)
        {
            try
            {
                // Convert the QR code string to a byte array
                byte[] qrCodeBytes = Convert.FromBase64String(qrCodeString);

                // Create a memory stream from the byte array
                using (MemoryStream memoryStream = new MemoryStream(qrCodeBytes))
                {
                    // Create a bitmap image from the memory stream
                    return new Bitmap(memoryStream);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string ConvertImageToBase64(Bitmap image)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Position = 0;

                byte[] imageBytes = memoryStream.ToArray();
                string base64String = Convert.ToBase64String(imageBytes);

                return base64String;
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
            public int PhoneNumber { get; set; }
        }



        //generate link qrcode in child phone
        public static string GenerateAndStoreQRCode(int childID)
        {
            // Generate QR code
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(childID.ToString(), QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);

            // Convert QR code to image
            Bitmap qrCodeImage = qrCode.GetGraphic(20);

            // Convert image to Base64 string
            string base64String;
            using (MemoryStream ms = new MemoryStream())
            {
                qrCodeImage.Save(ms, ImageFormat.Png);
                byte[] imageBytes = ms.ToArray();
                base64String = Convert.ToBase64String(imageBytes);
            }
            return base64String;
        }
    }
}
