using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace product_test
{
    public partial class Form2 : Form
    {
        // This DataTable acts as our temporary in-memory shopping cart
        private DataTable cartTable;
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            InitializeCart();
            LoadCategoryFilters();
            LoadDynamicProducts(null); // Load all products initially
        }

        #region Category Filtering Logic
        private void LoadCategoryFilters()
        {
            flpCategories.Controls.Clear();

            // 1. Create and add the default "All" button
            Button btnAll = new Button
            {
                Text = "All",
                Tag = null, // null tag represents "All"
                AutoSize = true,
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat
            };
            btnAll.Click += CategoryButton_Click;
            flpCategories.Controls.Add(btnAll);

            // 2. Fetch active categories from the database
            try
            {
                DataTable categories = DatabaseHelper.ExecuteQuery("SELECT CategoryID, CategoryName FROM Categories");
                foreach (DataRow row in categories.Rows)
                {
                    Button btnCat = new Button
                    {
                        Text = row["CategoryName"].ToString(),
                        Tag = Convert.ToInt32(row["CategoryID"]), // Store CategoryID in Tag
                        AutoSize = true,
                        BackColor = Color.WhiteSmoke,
                        FlatStyle = FlatStyle.Flat
                    };
                    btnCat.Click += CategoryButton_Click;
                    flpCategories.Controls.Add(btnCat);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load categories: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CategoryButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            int? categoryId = clickedButton.Tag as int?;

            // Highlight the selected filter button visually
            foreach (Control ctrl in flpCategories.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.BackColor = (btn == clickedButton) ? Color.LightSkyBlue : Color.WhiteSmoke;
                }
            }

            // Reload products filtered by selected Category
            LoadDynamicProducts(categoryId);
        }

        #endregion

        #region Product Display Logic

        private void LoadDynamicProducts(int? categoryId)
        {
            flpProducts.Controls.Clear();

            string query = "SELECT ProductID, ProductName, Price, ProductImage FROM Products";
            SqlParameter[] parameters = null;

            // If a specific category is selected, modify the query
            if (categoryId.HasValue)
            {
                query += " WHERE CategoryID = @CategoryID";
                parameters = new SqlParameter[] { new SqlParameter("@CategoryID", categoryId.Value) };
            }

            try
            {
                DataTable dt = DatabaseHelper.ExecuteQueryWithParams(query, parameters);

                foreach (DataRow row in dt.Rows)
                {
                    int productId = Convert.ToInt32(row["ProductID"]);
                    string productName = row["ProductName"].ToString();
                    decimal price = Convert.ToDecimal(row["Price"]);
                    byte[] imageBytes = row["ProductImage"] as byte[];

                    Panel productCard = CreateProductCard(productId, productName, price, imageBytes);
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
            Panel card = new Panel { Width = 180, Height = 260, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(10), BackColor = Color.White };

            PictureBox pbImage = new PictureBox { Width = 160, Height = 120, Location = new Point(10, 10), SizeMode = PictureBoxSizeMode.Zoom };
            pbImage.Image = (imageBytes != null && imageBytes.Length > 0) ? ConvertByteArrayToImage(imageBytes) : SystemIcons.Question.ToBitmap();

            Label lblName = new Label { Text = name, Location = new Point(10, 140), Width = 160, Height = 35, Font = new Font("Segoe UI", 10, FontStyle.Bold), TextAlign = ContentAlignment.TopCenter };
            Label lblPrice = new Label { Text = $"${price:F2}", Location = new Point(10, 180), Width = 160, Height = 20, Font = new Font("Segoe UI", 9, FontStyle.Regular), ForeColor = Color.DarkGreen, TextAlign = ContentAlignment.TopCenter };

            Button btnAdd = new Button { Text = "Add", Location = new Point(10, 210), Width = 75, Height = 30, BackColor = Color.LightGreen, FlatStyle = FlatStyle.Flat, Tag = id };
            btnAdd.Click += BtnAdd_Click;

            Button btnRemove = new Button { Text = "Remove", Location = new Point(95, 210), Width = 75, Height = 30, BackColor = Color.LightCoral, FlatStyle = FlatStyle.Flat, Tag = id };
            btnRemove.Click += BtnRemove_Click;

            card.Controls.Add(pbImage);
            card.Controls.Add(lblName);
            card.Controls.Add(lblPrice);
            card.Controls.Add(btnAdd);
            card.Controls.Add(btnRemove);

            return card;
        }

        private Image ConvertByteArrayToImage(byte[] byteArray)
        {
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                return Image.FromStream(ms);
            }
        }

        #endregion

        #region Shopping Cart Logic

        private void InitializeCart()
        {
            // Set up our schema inside the temporary DataTable
            cartTable = new DataTable();
            cartTable.Columns.Add("ProductID", typeof(int));
            cartTable.Columns.Add("Product", typeof(string));
            cartTable.Columns.Add("Quantity", typeof(int));
            cartTable.Columns.Add("Price", typeof(decimal));

            // This is a special column that automatically calculates Subtotal based on Quantity and Price
            cartTable.Columns.Add("Subtotal", typeof(decimal), "Quantity * Price");

            // Bind the table to the DataGridView
            dgvCart.DataSource = cartTable;

            // Hide ProductID from the user, but keep it in the background
            if (dgvCart.Columns["ProductID"] != null)
                dgvCart.Columns["ProductID"].Visible = false;

            // Simple styling for columns
            dgvCart.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCart.AllowUserToAddRows = false;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int productId = (int)btn.Tag;

            // Check if product is already in our cart
            DataRow[] existingRows = cartTable.Select($"ProductID = {productId}");

            if (existingRows.Length > 0)
            {
                // Increment the quantity
                int currentQty = Convert.ToInt32(existingRows[0]["Quantity"]);
                existingRows[0]["Quantity"] = currentQty + 1;
            }
            else
            {
                // Retrieve product details from database to add as a new row
                try
                {
                    DataTable itemDetails = DatabaseHelper.ExecuteQuery($"SELECT ProductName, Price FROM Products WHERE ProductID = {productId}");
                    if (itemDetails.Rows.Count > 0)
                    {
                        DataRow newRow = cartTable.NewRow();
                        newRow["ProductID"] = productId;
                        newRow["Product"] = itemDetails.Rows[0]["ProductName"];
                        newRow["Quantity"] = 1;
                        newRow["Price"] = itemDetails.Rows[0]["Price"];
                        cartTable.Rows.Add(newRow);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to retrieve item details: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            UpdateCartTotal();
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int productId = (int)btn.Tag;

            DataRow[] existingRows = cartTable.Select($"ProductID = {productId}");

            if (existingRows.Length > 0)
            {
                int currentQty = Convert.ToInt32(existingRows[0]["Quantity"]);
                if (currentQty > 1)
                {
                    // Decrement quantity
                    existingRows[0]["Quantity"] = currentQty - 1;
                }
                else
                {
                    // Remove item completely if quantity drops below 1
                    cartTable.Rows.Remove(existingRows[0]);
                }
            }

            UpdateCartTotal();
        }

        private void UpdateCartTotal()
        {
            decimal total = 0;

            foreach (DataRow row in cartTable.Rows)
            {
                total += Convert.ToDecimal(row["Subtotal"]);
            }

            lblTotal.Text = $"Total: ${total:F2}";
        }

        private void btnPlaceOrder_Click(object sender, EventArgs e)
        {
            if (cartTable.Rows.Count == 0)
            {
                MessageBox.Show("Your cart is empty. Please add items first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Create format-aligned preview of the order
            string receipt = "Order Preview:\n\n";
            receipt += string.Format("{0,-20} | {1,-8} | {2,-10}\n", "Product", "Qty", "Subtotal");
            receipt += new string('-', 45) + "\n";

            decimal grandTotal = 0;

            foreach (DataRow row in cartTable.Rows)
            {
                string productName = row["Product"].ToString();
                int qty = Convert.ToInt32(row["Quantity"]);
                decimal subtotal = Convert.ToDecimal(row["Subtotal"]);
                grandTotal += subtotal;

                receipt += string.Format("{0,-20} | {1,-8} | ${2,-10:F2}\n", productName, qty, subtotal);
            }

            receipt += new string('-', 45) + "\n";
            receipt += $"Total: ${grandTotal:F2}";

            // Display Receipt Preview
            MessageBox.Show(receipt, "Order Submitted Successfully!", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Reset the cart after successful order placement
            cartTable.Rows.Clear();
            UpdateCartTotal();
        }

        #endregion
    }
}

