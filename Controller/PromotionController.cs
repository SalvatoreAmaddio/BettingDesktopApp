using Betting.Model;
using Betting.View;
using Microsoft.Win32;
using MvvmHelpers.Commands;
using SARGUI;
using SARModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static SARGUI.View;

namespace Betting.Controller
{
    public class PromotionController : AbstractDataController<Promotion>
    {
        static BookMakerAccount? BkMkrAcc { get; set; }
        public ImageStorageManager? ImageStorageManager { get; set; }

        public PromotionController()
        {
            AllowNewRecord(false);           
            ImageStorageManager = new("Banner", 
            (IsDirty, newImgPath) =>
            {
                if (CurrentRecord == null) return;
                CurrentRecord.ImgPath = newImgPath;
                CurrentRecord.IsDirty = IsDirty;
            },
            
            () => CurrentRecord?.ImgPath, () => CurrentRecord == null,
            
            (val) =>
            {
                if (CurrentRecord == null) return;
                CurrentRecord.ImgPath = val?.ToString();
            });

            AfterUpdate += OnAfterUpdate;
        }
        private void OnAfterUpdate(object? sender, AbstractPropChangedEventArgs e)
        {
            if (e.PropIs(nameof(SelectedRecord)))
            {
                ImageStorageManager.Somethign();
                return;
            }

        //    ImageStorageManager.Something2();
            //if (e.PropIs(nameof(ImgSrc)))
            //{
            //    if (e.GetNewValue() == null && CurrentRecord != null)
            //    {
            //        CurrentRecord.ImgPath = null;
            //    }
            //    PlaceholderVisibility = (ImgSrc == null) ? Visibility.Visible : Visibility.Collapsed;
            //    return;
            //}

    //        if (e.PropIs(nameof(PlaceholderVisibility)))
    //        {
  //              ImageStorageManager.Something3((Visibility)e.GetNewValue());
//                Visibility value = (Visibility)e.GetNewValue();
//                ButtonVisibility = (value.Equals(Visibility.Visible)) ? Visibility.Hidden : Visibility.Visible;
 //               return;
     //       }
        }
        public override void OnRecordMoved(object? sender, RecordMovedEvtArgs e)
        {
            base.OnRecordMoved(sender, e);
            if (e.Record == null) return;
            ImageStorageManager?.ResetActionsAndFunctions(
            (IsDirty, newImgPath) => {
                if (CurrentRecord == null) return;
                CurrentRecord.ImgPath = newImgPath;
                CurrentRecord.IsDirty = IsDirty;
            },

            () => CurrentRecord?.ImgPath, () => CurrentRecord == null,

            (val) =>
            {
                if (CurrentRecord == null) return;
                CurrentRecord.ImgPath = val?.ToString();
            }
            );

            if (e.WentToNewRecord)
            {
                if (BkMkrAcc != null)
                {
                    Promotion promo = (Promotion)e.Record;
                    promo.BookMakerAccount = BkMkrAcc;
                    promo.IsDirty = false;
                    if (UIIsWindow())
                        GetUI<Window>().Title = $"New Promotion by {BkMkrAcc}";
                }
            }

            if (!e.Record.IsNewRecord && UIIsWindow())
                GetUI<Window>().Title = e.Record.ToString();
        }

        public override void OpenRecord(IAbstractModel record)
        {
            PromotionForm promotionForm = new(record);
            promotionForm.ShowDialog();
        }

        public override void OnAppearingGoTo(IAbstractModel record)
        {
            if (record is BookMakerAccount bookmaker)
            {
                BkMkrAcc = bookmaker;
                var range = MainSource.Where(s => ((Promotion)s).BookMakerAccount.IsEqualTo(record));
                RecordSource.ReplaceData(range);
                base.OnAppearingGoTo((ChildSource.RecordCount > 0) ? ChildSource.First() : new Promotion(BkMkrAcc));
                return;
            }
            base.OnAppearingGoTo(record);
        }


        public override bool OnFormClosing(CancelEventArgs e)
        {
            AllowNewRecord(false);
            return base.OnFormClosing(e);
        }

        public override bool Delete(IAbstractModel? record)
        {
            string? p= (record!=null) ? ((Promotion)record).ImgPath : string.Empty;

            Task task = new(() =>
            {
                ImageStorageManager?.OnRecordDeleted(p);
            });

            bool result = base.Delete(record);
            if (result) task.Start();
            return result;
        }

        public override bool Save(IAbstractModel? record)
        {
            if (record != null && BkMkrAcc != null)
                ((Promotion)record).BookMakerAccount = BkMkrAcc;

            bool result = base.Save(record);
            if (UIIsWindow())
                GetUI<Window>().Title = record?.ToString();

            ImageStorageManager.UpdateStorage();

            if (BkMkrAcc != null) 
            {
                AccountHolderBookMakerAccount AccHldrBkMkAcc = new(BkMkrAcc);
                BetController.RunRefreshOnBookMakerChangedTask(AccHldrBkMkAcc);            
            }

            return result;
        }
    }


}
