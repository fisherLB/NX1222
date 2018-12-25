using System;
using System.IO;
using System.Drawing;
using System.Web;
using System.Web.UI.WebControls;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

namespace JN.Services.Tool
{
    public class UploadPic : System.Web.UI.Page
    {
        #region WebForm控件上传文件,返回文件名
        /// <summary>
        /// WebForm控件上传文件,生成日期文件夹,返回文件名
        /// </summary>
        /// <param name="myFileUpload">上传控件ID</param>
        /// <param name="allowExtensions">允许上传的扩展文件名类型,如：string[] allowExtensions = { ".doc", ".xls", ".ppt", ".jpg", ".gif" };</param>
        /// <param name="maxLength">允许上传的最大大小，以M为单位</param>
        /// <param name="savePath">保存文件的目录，注意是绝对路径,如：Server.MapPath("~/upload/");</param>
        public static string Upload(FileUpload myFileUpload, string[] allowExtensions, int maxLength, string savePath)
        {
            return Upload(myFileUpload, allowExtensions, maxLength, savePath, false);
        }

        /// <summary>
        /// WebForm控件上传文件,返回文件名
        /// </summary>
        /// <param name="myFileUpload">上传控件ID</param>
        /// <param name="allowExtensions">允许上传的扩展文件名类型,如：string[] allowExtensions = { ".doc", ".xls", ".ppt", ".jpg", ".gif" };</param>
        /// <param name="maxLength">允许上传的最大大小，以M为单位</param>
        /// <param name="savePath">保存文件的目录，注意是绝对路径,如：Server.MapPath("~/upload/");</param>
        /// <param name="gendatedir">是否生成日期文件夹</param>
        public static string Upload(FileUpload myFileUpload, string[] allowExtensions, int maxLength, string savePath, bool gendatedir)
        {
            // 文件格式是否允许上传
            bool fileAllow = false;

            //检查是否有文件案
            if (myFileUpload.HasFile)
            {
                // 检查文件大小, ContentLength获取的是字节，转成M的时候要除以2次1024
                if (myFileUpload.PostedFile.ContentLength / 1024 / 1024 >= maxLength)
                {
                    throw new Exception("只能上传小于" + maxLength + "M的文件！");
                }

                //取得上传文件之扩展文件名，并转换成小写字母
                string fileExtension = System.IO.Path.GetExtension(myFileUpload.FileName).ToLower();
                string tmp = "";   // 存储允许上传的文件后缀名
                //检查扩展文件名是否符合限定类型
                for (int i = 0; i < allowExtensions.Length; i++)
                {
                    tmp += i == allowExtensions.Length - 1 ? allowExtensions[i] : allowExtensions[i] + ",";
                    if (fileExtension == allowExtensions[i])
                    {
                        fileAllow = true;
                    }
                }

                if (fileAllow)
                {
                    try
                    {
                        string datedir = DateTime.Now.ToString("yyyyMMdd");
                        if (!Directory.Exists(savePath + datedir) && gendatedir)
                        {
                            Directory.CreateDirectory(savePath + datedir);
                        }
                        if (!gendatedir && !Directory.Exists(savePath))
                        {
                            Directory.CreateDirectory(savePath);
                        }
                        string saveName = Guid.NewGuid() + fileExtension;
                        string path = gendatedir ? savePath + datedir + "/" + saveName : savePath + saveName;
                        //存储文件到文件夹
                        myFileUpload.SaveAs(path);
                        return gendatedir ? datedir + "/" + saveName : saveName;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                else
                {
                    throw new Exception("文件格式不符，可以上传的文件格式为：" + tmp);
                }
            }
            else
            {
                throw new Exception("请选择要上传的文件！");
            }
        }
        #endregion

        #region MVC上传文件,生成日期文件夹,返回文件名
        /// <summary>
        /// MVC上传文件上传文件,返回文件名
        /// </summary>
        /// <param name="myFileUpload">上传控件ID</param>
        /// <param name="allowExtensions">允许上传的扩展文件名类型,如：string[] allowExtensions = { ".doc", ".xls", ".ppt", ".jpg", ".gif" };</param>
        /// <param name="maxLength">允许上传的最大大小，以M为单位</param>
        /// <param name="savePath">保存文件的目录，注意是绝对路径,如：Server.MapPath("~/upload/");</param>
        public static string MvcUpload(HttpPostedFileBase myFileUpload, string[] allowExtensions, int maxLength, string savePath)
        {
            // 文件格式是否允许上传
            bool fileAllow = false;

            //检查是否有文件案
            if (myFileUpload != null)
            {
                // 检查文件大小, ContentLength获取的是字节
                if (myFileUpload.ContentLength >= maxLength)
                {
                    throw new Exception("只能上传小于" + maxLength / 1024 + "KB的文件！");
                }

                //取得上传文件之扩展文件名，并转换成小写字母
                string fileExtension = System.IO.Path.GetExtension(myFileUpload.FileName).ToLower();
                string tmp = "";   // 存储允许上传的文件后缀名
                //检查扩展文件名是否符合限定类型
                for (int i = 0; i < allowExtensions.Length; i++)
                {
                    tmp += i == allowExtensions.Length - 1 ? allowExtensions[i] : allowExtensions[i] + ",";
                    if (fileExtension == allowExtensions[i])
                    {
                        fileAllow = true;
                    }
                }

                if (fileAllow)
                {
                    if (!Directory.Exists(savePath))
                    {
                        Directory.CreateDirectory(savePath);
                    }
                    if (!Directory.Exists(savePath))
                    {
                        Directory.CreateDirectory(savePath);
                    }
                    string saveName = Guid.NewGuid() + fileExtension;
                    string path = savePath + saveName;
                    //存储文件到文件夹
                    myFileUpload.SaveAs(path);
                    return saveName;
                }
                else
                {
                    throw new Exception("文件格式不符，可以上传的文件格式为：" + tmp);
                }
            }
            else
            {
                throw new Exception("请选择要上传的文件！");
            }
        }
        #endregion

        #region 按照最大高宽来生成等比例缩略图,并以图片的形式输出

        //按照最大高宽来生成等比例缩略图,并以图片的形式输出

        public static string MyThumbnail(string originalImagePath, string thumbnailPath, int width, int height)
        {
            return MakeThumbnail(originalImagePath, thumbnailPath, width, height, "EQU");
        }

        /// <summary>
        /// 按照最大高宽来生成等比例缩略图,并以图片的形式输出
        /// </summary>
        /// <param name="newWidth">缩略图最大宽度</param>
        /// <param name="newHeight">缩略图最大高度</param>
        /// <param name="oldPath">原始图像地址，可以是绝度地址也可以是相对地址</param>
        /// <returns></returns>
        public void CreateThumbnail(int newWidth, int newHeight, string oldPath)
        {
            try
            {
                int oldwidth, oldheight, neww, newh;
                System.Drawing.Image image;
                System.Drawing.Image.GetThumbnailImageAbort callb = new System.Drawing.Image.GetThumbnailImageAbort(ThumbnailCallback);
                image = GetImage(oldPath);
                oldwidth = image.Width;
                oldheight = image.Height;

                if (newWidth > oldwidth && newHeight > oldheight)//如果缩略图宽高都大于原始图宽高，则按原始图尺寸输出
                {
                    neww = oldwidth;
                    newh = oldheight;
                }
                else if (oldwidth * newHeight > newWidth * oldheight)
                {
                    neww = newWidth;
                    newh = oldheight * newWidth / oldwidth;
                }
                else
                {
                    newh = newHeight;
                    neww = oldwidth * newHeight / oldheight;
                }
                //newimage = image.GetThumbnailImage(neww, newh, callb, IntPtr.Zero);
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(neww, newh);
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                //设置高质量插值法 
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                //设置高质量,低速度呈现平滑程度 
                g.Clear(Color.Transparent); //清空画布并以透明背景色填充 
                g.DrawImage(image, 0, 0, bmp.Width, bmp.Height);

                //输出到页面
                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                Response.ClearContent(); //需要输出图象信息 要修改HTTP头 
                byte[] buffer = ms.ToArray();

                Response.AddHeader("Content-type", "image/jpeg");
                Response.BinaryWrite(buffer);

                image.Dispose();
                Response.End();
            }
            catch (Exception e)
            {
                // return false;
                Response.Write("图片处理错误");
                // Response.Write(e.Message);
            }
        }

        private bool ThumbnailCallback()
        {
            return false;
        }

        public System.Drawing.Image GetImage(string path)
        {
            if (path.StartsWith("http"))//获取远程图片
            {
                System.Net.WebRequest request = System.Net.WebRequest.Create(path);
                request.Timeout = 10000;
                System.Net.HttpWebResponse httpresponse = (System.Net.HttpWebResponse)request.GetResponse();
                Stream s = httpresponse.GetResponseStream();
                return System.Drawing.Image.FromStream(s);
            }
            else//获取本地图片
            {
                return System.Drawing.Image.FromFile(Server.MapPath(path));
            }
        }
        #endregion

        #region 图片缩放，多种指定方式生成图片
        /// <summary>
        /// 图片缩放
        /// </summary>
        /// <param name="originalImagePath">原始图片路径，如：c:\\images\\1.gif</param>
        /// <param name="thumbnailPath">生成缩略图图片路径，如：c:\\images\\2.gif</param>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <param name="mode">EQU：指定最大高宽等比例缩放；HW：//指定高宽缩放（可能变形）；W:指定宽，高按比例；H:指定高，宽按比例；Cut：指定高宽裁减（不变形）</param>
        public static string MakeThumbnail(string originalImagePath, string thumbnailPath, int width, int height, string mode)
        {
            FileInfo fi = new FileInfo(thumbnailPath);
            if (fi != null)
            {
                if (!Directory.Exists(fi.DirectoryName))
                {
                    Directory.CreateDirectory(fi.DirectoryName);
                }
            }
            System.Drawing.Image originalImage = System.Drawing.Image.FromFile(originalImagePath);

            int towidth = width;
            int toheight = height;

            int x = 0;
            int y = 0;
            int ow = originalImage.Width;
            int oh = originalImage.Height;

            if (mode == "EQU")//指定最大高宽，等比例缩放
            {
                //if(height/oh>width/ow),如果高比例多，按照宽来缩放；如果宽的比例多，按照高来缩放
                if (ow >= oh)
                {
                    mode = "W";
                }
                else
                {
                    mode = "H";
                }
            }
            switch (mode)
            {
                case "HW"://指定高宽缩放（可能变形）                   
                    break;
                case "W"://指定宽，高按比例                       
                    toheight = originalImage.Height * width / originalImage.Width;
                    break;
                case "H"://指定高，宽按比例   
                    towidth = originalImage.Width * height / originalImage.Height;
                    break;
                case "Cut"://指定高宽裁减（不变形）                   
                    if ((double)originalImage.Width / (double)originalImage.Height > (double)towidth / (double)toheight)
                    {
                        oh = originalImage.Height;
                        ow = originalImage.Height * towidth / toheight;
                        y = 0;
                        x = (originalImage.Width - ow) / 2;
                    }
                    else
                    {
                        ow = originalImage.Width;
                        oh = originalImage.Width * height / towidth;
                        x = 0;
                        y = (originalImage.Height - oh) / 2;
                    }
                    break;
                default:
                    break;
            }

            //新建一个bmp图片   
            System.Drawing.Image bitmap = new System.Drawing.Bitmap(towidth, toheight);

            //新建一个画板   
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);

            //设置高质量插值法   
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

            //设置高质量,低速度呈现平滑程度   
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充   
            g.Clear(System.Drawing.Color.Transparent);

            //在指定位置并且按指定大小绘制原图片的指定部分   
            g.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, towidth, toheight),
                new System.Drawing.Rectangle(x, y, ow, oh),
                System.Drawing.GraphicsUnit.Pixel);
            try
            {
                //以jpg格式保存缩略图   
                string newName = System.IO.Path.GetFileName(originalImagePath).ToLower() + "_" + width + "-" + height + ".jpg";
                bitmap.Save(thumbnailPath + newName, System.Drawing.Imaging.ImageFormat.Jpeg);
                return newName;
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                g.Dispose();
            }
        }
        #endregion

        #region 图片相关的其他方法
        /**/
        /// <summary>   
        /// 在图片上增加文字水印   
        /// </summary>   
        /// <param name="path">原服务器图片路径</param>   
        /// <param name="pathSy">生成的带文字水印的图片路径</param>   
        protected void AddShuiYinWord(string path, string pathSy)
        {
            string addText = "Mega Elte";
            System.Drawing.Image image = System.Drawing.Image.FromFile(path);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(image);
            g.DrawImage(image, 0, 0, image.Width, image.Height);
            System.Drawing.Font f = new System.Drawing.Font("Verdana", 16);
            System.Drawing.Brush b = new System.Drawing.SolidBrush(System.Drawing.Color.Gray);
            g.DrawString(addText, f, b, 15, 15);
            g.Dispose();
            image.Save(pathSy);
            image.Dispose();
        }

        /**/
        /// <summary>   
        /// 在图片上生成图片水印
        /// </summary>   
        /// <param name="path">原服务器图片路径</param>   
        /// <param name="pathSyp">生成的带图片水印的图片路径</param>   
        /// <param name="pathSypf">水印图片路径</param>   
        protected void AddShuiYinPic(string path, string pathSyp, string pathSypf)
        {
            System.Drawing.Image image = System.Drawing.Image.FromFile(path);
            System.Drawing.Image copyImage = System.Drawing.Image.FromFile(pathSypf);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(image);
            g.DrawImage(copyImage, new System.Drawing.Rectangle(image.Width - copyImage.Width, image.Height - copyImage.Height, copyImage.Width, copyImage.Height), 0, 0, copyImage.Width, copyImage.Height, System.Drawing.GraphicsUnit.Pixel);
            g.Dispose();
            image.Save(pathSyp);
            image.Dispose();
        }

        protected static void AddImageSignText(System.Drawing.Image img, string filename, string watermarkText, int watermarkPosition)
        {
            Graphics g = Graphics.FromImage(img);
            //设置高质量插值法
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

            //设置高质量,低速度呈现平滑程度
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            Font drawFont = new Font("Tahoma", ((float)img.Width * (float).03), FontStyle.Regular, GraphicsUnit.Pixel);
            SizeF crSize;
            crSize = g.MeasureString(watermarkText, drawFont);

            float xpos = 0;
            float ypos = 0;

            switch (watermarkPosition)
            {
                case 3:
                    xpos = ((float)img.Width * (float).50) - (crSize.Width / 2);
                    ypos = ((float)img.Height * (float).50) - (crSize.Height / 2);
                    break;
                case 2:
                    xpos = ((float)img.Width * (float).50) - (crSize.Width / 2);
                    ypos = (float)img.Height - crSize.Height;
                    break;
                case 1:
                    xpos = ((float)img.Width * (float).99) - crSize.Width;
                    ypos = (float)img.Height - crSize.Height;
                    break;
            }

            g.DrawString(watermarkText, drawFont, new SolidBrush(Color.White), xpos + 1, ypos + 1);
            g.DrawString(watermarkText, drawFont, new SolidBrush(Color.Black), xpos, ypos);
            img.Save(filename);
            g.Dispose();
            img.Dispose();
        }


        /// <summary>
        /// 替换字符串中的远程文件为本地文件并保存远程文件
        /// </summary>
        /// <param name="savedir">本地保存路径</param>
        /// <param name="imgpath">远程图片文件</param>
        /// <returns></returns>
        public static string ReplaceRemoteUrl(string SaveFilePath, string strContent)
        {
            string pattern = @"((http|https|ftp|rtsp|mms):(\/\/|\\\\){1}((\w)+[.]){1,}(net|com|cn|org|cc|tv|[0-9]{1,3})(\S*\/)((\S)+[.]{1}(gif|jpg|png|bmp)))";
            foreach (Match m in Regex.Matches(strContent, pattern))
            {
                // 取得匹配的字符串  
                string x = m.ToString();
                string SaveFileType = GetFileExtName(x);
                //Random ran = new Random();
                //int RandKey = ran.Next(100, 999);
                //string SaveFileName = SaveFilePath + string.Format("{0:yyyyMMddHHmmssffff}", DateTime.Now) + RandKey.ToString() + SaveFileType;
                string SaveFileName = downRemoteImg(SaveFilePath, x);
                strContent = strContent.Replace(x, SaveFilePath + SaveFileName);
            }  
            return strContent;
        }

        /// <summary>
        /// 获取指定文件的扩展名
        /// </summary>
        /// <param name="fileName">指定文件名</param>
        /// <returns>扩展名</returns>
        public static string GetFileExtName(string fileName)
        {
            if (String.IsNullOrEmpty(fileName) || fileName.IndexOf('.') <= 0)
                return "";

            fileName = fileName.ToLower().Trim();
            return fileName.Substring(fileName.LastIndexOf('.'), fileName.Length - fileName.LastIndexOf('.'));
        }

        /// <summary>
        /// 下载远程图片保存到本地
        /// </summary>
        /// <param name="savedir">本地保存路径</param>
        /// <param name="imgpath">远程图片文件</param>
        /// <returns></returns>
        public static string downRemoteImg(string savedir, string imgpath)
        {
            if (string.IsNullOrEmpty(imgpath))
                return string.Empty;
            else
            {
                //string imgName = string.Empty;
                string imgExt = string.Empty;
                string saveFilePath = string.Empty;


                //imgName = imgpath.Substring(imgpath.LastIndexOf("/"), imgpath.Length - imgpath.LastIndexOf("/"));
                imgExt = imgpath.Substring(imgpath.LastIndexOf("."), imgpath.Length - imgpath.LastIndexOf("."));

                string saveName = Guid.NewGuid() + imgExt;

                saveFilePath = System.Web.HttpContext.Current.Server.MapPath(savedir);
                if (!Directory.Exists(saveFilePath))
                    Directory.CreateDirectory(saveFilePath);
                try
                {
                    System.Net.WebRequest wreq = System.Net.WebRequest.Create(imgpath);
                    wreq.Timeout = 10000;
                    System.Net.HttpWebResponse wresp = (System.Net.HttpWebResponse)wreq.GetResponse();
                    Stream s = wresp.GetResponseStream();
                    System.Drawing.Image img;
                    img = System.Drawing.Image.FromStream(s);
                    switch (imgExt.ToLower())
                    {
                        case ".gif":
                            img.Save(saveFilePath + saveName, ImageFormat.Gif);
                            break;
                        case ".jpg":
                        case ".jpeg":
                            img.Save(saveFilePath + saveName, ImageFormat.Jpeg);
                            break;
                        case ".png":
                            img.Save(saveFilePath + saveName, ImageFormat.Png);
                            break;
                        case ".icon":
                            img.Save(saveFilePath + saveName, ImageFormat.Icon);
                            break;
                        case ".bmp":
                            img.Save(saveFilePath + saveName, ImageFormat.Bmp);
                            break;
                    }

                    img.Dispose();
                    s.Dispose();

                    return saveName;
                }
                catch
                {
                    return "";
                }
            }
        }
        #endregion
    }
}
