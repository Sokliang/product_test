using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace product_test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadDynamicProducts();
        }
        private void LoadDynamicProducts()
        {
            // Clear any existing cards in the panel first
            flpProducts.Controls.Clear();

            string query = "SELECT ProductID, ProductName, Price, ProductImage FROM Products";

            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery(query);

                foreach (DataRow row in dt.Rows)
                {
                    int productId = Convert.ToInt32(row["ProductID"]);
                    string productName = row["ProductName"].ToString();
                    decimal price = Convert.ToDecimal(row["Price"]);

                    // Retrieve image byte array
                    byte[] imageBytes = row["ProductImage"] as byte[];

                    // Create the physical card container
                    Panel productCard = CreateProductCard(productId, productName, price, imageBytes);

                    // Add the card to the FlowLayoutPanel
                    flpProducts.Controls.Add(productCard);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Panel CreateProductCard(int id, string name, decimal price, byte[] imageBytes)
        {
            // 1. Main Card Panel
            Panel card = new Panel
            {
                Width = 180,
                Height = 260,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10),
                BackColor = Color.White
            };

            // 2. PictureBox for Product Image
            PictureBox pbImage = new PictureBox
            {
                Width = 160,
                Height = 120,
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom
            };

            if (imageBytes != null && imageBytes.Length > 0)
            {
                pbImage.Image = ConvertByteArrayToImage(imageBytes);
            }
            else
            {
                // Set a default placeholder if there is no image in the database
                pbImage.Image = SystemIcons.Question.ToBitmap();
            }

            // 3. Label for Product Name
            Label lblName = new Label
            {
                Text = name,
                Location = new Point(10, 140),
                Width = 160,
                Height = 35,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.TopCenter
            };

            // 4. Label for Price
            Label lblPrice = new Label
            {
                Text = $"${price:F2}",
                Location = new Point(10, 180),
                Width = 160,
                Height = 20,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.DarkGreen,
                TextAlign = ContentAlignment.TopCenter
            };

            // 5. Add to Order Button
            Button btnAdd = new Button
            {
                Text = "Add",
                Location = new Point(10, 210),
                Width = 75,
                Height = 30,
                BackColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat,
                Tag = id // Store the ProductID in the Tag so we know which product was clicked
            };
            btnAdd.Click += BtnAdd_Click;

            // 6. Remove Button
            Button btnRemove = new Button
            {
                Text = "Remove",
                Location = new Point(95, 210),
                Width = 75,
                Height = 30,
                BackColor = Color.LightCoral,
                FlatStyle = FlatStyle.Flat,
                Tag = id // Store the ProductID here too
            };
            btnRemove.Click += BtnRemove_Click;

            // Add all child controls into the main card panel
            card.Controls.Add(pbImage);
            card.Controls.Add(lblName);
            card.Controls.Add(lblPrice);
            card.Controls.Add(btnAdd);
            card.Controls.Add(btnRemove);

            return card;
        }

        // Helper method to convert the VARBINARY bytes back into a usable Image object
        private Image ConvertByteArrayToImage(byte[] byteArray)
        {
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                return Image.FromStream(ms);
            }
        }

        // Event handler for Add button click
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            int productId = (int)clickedButton.Tag;

            // Handle your "Add to Order" logic here
            MessageBox.Show($"Product ID {productId} added to order.");
        }

        // Event handler for Remove button click
        private void BtnRemove_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            int productId = (int)clickedButton.Tag;

            // Handle your "Remove from Order" logic here
            MessageBox.Show($"Product ID {productId} removed from order.");
        }
    }
}
