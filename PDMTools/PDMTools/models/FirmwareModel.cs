using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using PDMTools.defined;
using PDMTools.datas;

namespace PDMTools.models
{
    /*
     * 管理固件发行文件相关数据及对应UI显示
     */
    public class FirmwareModel : BaseModel
    {
        private CheckBox mPublishFirmwareCb = null;
        private Label mFirmwareImgLabel = null;
        private Label mFirmwareZipLabel = null;
        private Button mFirmwareImgBtn = null;
        private Button mFirmwareZipBtn = null;

        public override void init(MainWindow win)
        {
            base.init(win);
            mPublishFirmwareCb = win.PublishFirmware;
            mFirmwareImgLabel = win.SelectImgFileLabel;
            mFirmwareZipLabel = win.SelectZipFileLabel;
            mFirmwareImgBtn = win.SelectImgFileBtn;
            mFirmwareZipBtn = win.SelectZipFileBtn;
            showState(Defined.UiState.Idle);
        }

        public override void deinit()
        {
            mFirmwareZipBtn = null;
            mFirmwareImgBtn = null;
            mFirmwareZipLabel = null;
            mFirmwareImgLabel = null;
            base.deinit();
        }

        public override void showState(Defined.UiState state)
        {
            if (!isInited())
            {
                return;
            }

            switch (state)
            {
                case Defined.UiState.SelectedFirmware:
                case Defined.UiState.SelectedFirmwareAndTool:
                    {
                        mPublishFirmwareCb.IsEnabled = true;
                        mFirmwareImgBtn.IsEnabled = true;
                        mFirmwareZipBtn.IsEnabled = true;
                    }
                    break;

                case Defined.UiState.SelectedTemplate:
                case Defined.UiState.SelectedTool:
                    {
                        mPublishFirmwareCb.IsEnabled = true;
                        mFirmwareImgBtn.IsEnabled = false;
                        mFirmwareZipBtn.IsEnabled = false;
                    }
                    break;

                case Defined.UiState.Doing:
                    {
                        mPublishFirmwareCb.IsEnabled = false;
                        mFirmwareImgBtn.IsEnabled = false;
                        mFirmwareZipBtn.IsEnabled = false;
                    }
                    break;

                case Defined.UiState.Idle:
                default:
                    {
                        mFirmwareImgLabel.IsEnabled = true;
                        mFirmwareZipLabel.IsEnabled = true;
                        mFirmwareImgLabel.Content = mWin.FindResource("please_select_img_file");
                        mFirmwareZipLabel.Content = mWin.FindResource("please_select_zip_file");

                        mFirmwareImgBtn.IsEnabled = false;
                        mFirmwareZipBtn.IsEnabled = false;
                        
                        mPublishFirmwareCb.IsEnabled = false;
                        mPublishFirmwareCb.IsChecked = false;
                    }
                    break;
            }
        }

        public override bool isValid(Defined.UiState state)
        {
            if (!isInited())
            {
                return false;
            }

            switch (state)
            {
                case Defined.UiState.SelectedFirmware:
                case Defined.UiState.SelectedFirmwareAndTool:
                    {
                        if (isValidImgFile(mFirmwareImgLabel.Content.ToString()) &&
                            isValidZipFile(mFirmwareZipLabel.Content.ToString()))
                        {
                            return true;
                        }
                    }
                    break;

                case Defined.UiState.SelectedTemplate:
                case Defined.UiState.SelectedTool:
                case Defined.UiState.Doing:
                case Defined.UiState.Idle:
                default:
                    break;
            }

            return false;
        }

        public override List<Operate> getOperates()
        {
            if (!isInited())
            {
                return null;
            }

            // 只生成列表, 不具体计算, 避免主线程开销
            List<Operate> list = new List<Operate>();

            // Img file
            Operate curOp = new Operate();
            curOp.type = Defined.OperateType.CalcFileVersion;
            curOp.key = Defined.KeyName.ImgFileVersion.ToString();
            curOp.value = mFirmwareImgLabel.Content.ToString();
            list.Add(curOp);

            curOp = new Operate();
            curOp.type = Defined.OperateType.CalcFileMd5;
            curOp.key = Defined.KeyName.ImgFileMd5.ToString();
            curOp.value = mFirmwareImgLabel.Content.ToString();
            list.Add(curOp);

            curOp = new Operate();
            curOp.type = Defined.OperateType.CalcFileModifiedTime;
            curOp.key = Defined.KeyName.ImgFileModifiedTime.ToString();
            curOp.value = mFirmwareImgLabel.Content.ToString();
            list.Add(curOp);

            curOp = new Operate();
            curOp.type = Defined.OperateType.CalcFileSizeByBytes;
            curOp.key = Defined.KeyName.ImgFileSize.ToString();
            curOp.value = mFirmwareImgLabel.Content.ToString();
            list.Add(curOp);

            curOp = new Operate();
            curOp.type = Defined.OperateType.CalcFileSizeByM;
            curOp.key = Defined.KeyName.ImgFileSizeByManual.ToString();
            curOp.value = mFirmwareImgLabel.Content.ToString();
            list.Add(curOp);

            // Zip file
            curOp = new Operate();
            curOp.type = Defined.OperateType.CalcFileVersion;
            curOp.key = Defined.KeyName.ZipFileMd5.ToString();
            curOp.value = mFirmwareZipLabel.Content.ToString();
            list.Add(curOp);

            curOp = new Operate();
            curOp.type = Defined.OperateType.CalcFileMd5;
            curOp.key = Defined.KeyName.ZipFileMd5.ToString();
            curOp.value = mFirmwareZipLabel.Content.ToString();
            list.Add(curOp);

            curOp = new Operate();
            curOp.type = Defined.OperateType.CalcFileModifiedTime;
            curOp.key = Defined.KeyName.ZipFileModifiedTime.ToString();
            curOp.value = mFirmwareZipLabel.Content.ToString();
            list.Add(curOp);

            curOp = new Operate();
            curOp.type = Defined.OperateType.CalcFileSizeByBytes;
            curOp.key = Defined.KeyName.ZipFileSize.ToString();
            curOp.value = mFirmwareZipLabel.Content.ToString();
            list.Add(curOp);

            curOp = new Operate();
            curOp.type = Defined.OperateType.CalcFileSizeByM;
            curOp.key = Defined.KeyName.ZipFileSizeByManual.ToString();
            curOp.value = mFirmwareZipLabel.Content.ToString();
            list.Add(curOp);

            return list;
        }

        public bool isNeedPublish()
        {
            if (!isInited())
            {
                return false;
            }

            return (bool)mPublishFirmwareCb.IsChecked;
        }

        public void setSelectedImgFile(string selectedfile)
        {
            if (!isInited())
            {
                return;
            }

            mFirmwareImgLabel.Content = selectedfile;
        }

        public void resetSelectedImgFile()
        {
            if (!isInited())
            {
                return;
            }

            mFirmwareImgLabel.Content = mWin.FindResource("please_select_img_file");
        }

        public bool isValidImgFile(string file)
        {
            if (!isInited())
            {
                return false;
            }

            if (!System.IO.File.Exists(file))
            {
                return false;
            }

            string filename = System.IO.Path.GetFileNameWithoutExtension(file);
            string extension = System.IO.Path.GetExtension(file);
            if (!extension.Equals(".img"))
            {
                return false;
            }

            Regex rgx = new Regex(@"[\d]{1,4}[.][\d]{1,4}[.][\d]{1,4}[.][\d]{1,4}");
            return rgx.IsMatch(filename);
        }

        public void setSelectedZipFile(string selectedfile)
        {
            if (!isInited())
            {
                return;
            }

            mFirmwareZipLabel.Content = selectedfile;
        }

        public void resetSelectedZipFile()
        {
            if (!isInited())
            {
                return;
            }

            mFirmwareZipLabel.Content = mWin.FindResource("please_select_zip_file");
        }

        public bool isValidZipFile(string file)
        {
            if (!isInited())
            {
                return false;
            }

            if (!System.IO.File.Exists(file))
            {
                return false;
            }

            string filename = System.IO.Path.GetFileNameWithoutExtension(file);
            string extension = System.IO.Path.GetExtension(file);
            if (!extension.Equals(".zip"))
            {
                return false;
            }

            Regex rgx = new Regex(@"[\d]{1,4}[.][\d]{1,4}[.][\d]{1,4}[.][\d]{1,4}");
            return rgx.IsMatch(filename);
        }
    }
}
