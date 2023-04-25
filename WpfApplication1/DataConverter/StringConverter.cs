using System;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using View.CommonEnum;

namespace View.DataConverter.StringConverter {
    class StringConverter {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="is24"></param>
        /// <returns></returns>
        static public string GetRecordDatTimeStringConverter(DateTime? data, bool is24) {
            string dateformat = string.Empty;
            string value = string.Empty;
            DateTime workdata = new DateTime();
            if ( data != null ) {
                workdata = (DateTime)data;
                if ( is24 ) {
                    dateformat = Application.Current.TryFindResource("RECORD_DATETIME") as string;
                    value = string.Format(dateformat, 
                                            new object[] {workdata.Year, workdata.Month, workdata.Day,
                                                          workdata.Hour, workdata.Minute, workdata.Second});
            
                } else {
                    if ( workdata.Hour >= 12 ) {
                        dateformat = Application.Current.TryFindResource("RECORD_DATETIME_PM") as string;
                        value = string.Format(dateformat, 
                                            new object[] {workdata.Year, workdata.Month, workdata.Day,
                                                          workdata.Hour - 12, workdata.Minute, workdata.Second});
                    } else {
                        dateformat = Application.Current.TryFindResource("RECORD_DATETIME_AM") as string;
                        value = string.Format(dateformat, 
                                            new object[] {workdata.Year, workdata.Month, workdata.Day,
                                                          workdata.Hour, workdata.Minute, workdata.Second});
                    }
                }
            } else {
                value = string.Empty;
            }
            return value;
        }

        static public string GetRecordDateStringConverter(DateTime? data) {
            string dateformat = string.Empty;
            string value = string.Empty;
            dateformat = Application.Current.TryFindResource("RECORD_DATE") as string;
            DateTime workdata = new DateTime();
            if ( data != null ) {
                workdata = (DateTime)data;
                value = string.Format(dateformat, 
                                    new object[] {workdata.Year, workdata.Month, workdata.Day});
            } else {
                value = "-";
            }
            return value;
        }

        static public string GetNumOfListStringConverter(int count) {
                return string.Format(
                            Application.Current.TryFindResource("NUM_OF_RECORD") as string,
                            count);
        }

        static public string GetNumOfListStringConverter2(int count, int totalcount) {
                return string.Format(
                            Application.Current.TryFindResource("NUM_OF_RECORD2") as string,
                            count, totalcount);
        }

        static public string GetRecordTimeStringConverter(DateTime? data, bool is24) {
            string dateformat = string.Empty;
            string value = string.Empty;
            DateTime workdata = new DateTime();
            if ( data != null ) {
                workdata = (DateTime)data;
                if ( is24 ) {
                    dateformat = Application.Current.TryFindResource("RECORD_TIME") as string;
                    value = string.Format(dateformat, 
                                            new object[] {workdata.Hour, workdata.Minute});
            
                } else {
                    if ( workdata.Hour >= 12 ) {
                        dateformat = Application.Current.TryFindResource("RECORD_TIME_PM") as string;
                        value = string.Format(dateformat, 
                                            new object[] {workdata.Hour - 12, workdata.Minute});
                    } else {
                        dateformat = Application.Current.TryFindResource("RECORD_TIME_AM") as string;
                        value = string.Format(dateformat, 
                                            new object[] {workdata.Hour, workdata.Minute});
                    }
                }
            } else {
                value = "-";
            }
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        static public string GetRecDurationTimeStringCoverter(TimeSpan? data, bool isJp) {
            string timeformat = string.Empty;
            string value = string.Empty;
            string normal = string.Empty;
            TimeSpan workdata = new TimeSpan();
            if ( data != null ) {
                workdata = (TimeSpan)data;
                if (isJp) {
                    normal = "_NOR";
                }
                if ( workdata.Hours > 0 ) {
                    timeformat = Application.Current.TryFindResource("RECDURATION_TIME_HOUR" + normal) as string;
                    value = string.Format(timeformat, 
                                            new object[] {workdata.Hours, workdata.Minutes, workdata.Seconds});
                } else {
                    timeformat = Application.Current.TryFindResource("RECDURATION_TIME" + normal) as string;
                    value = string.Format(timeformat, new object[] {workdata.Minutes, workdata.Seconds});
                }
            } else {
                value = string.Empty;
            }
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        static public string GetDayOfWeeksStringConverter(DayOfWeek data) {
            return Application.Current.TryFindResource(data.ToString().ToUpper()) as string;
        }

        static public void InitRecModeComboBox(ItemCollection items) {
            items.Clear();
            items.Add(Application.Current.TryFindResource(RecMode.ALL.ToString()) as string);
            items.Add(Application.Current.TryFindResource(RecMode.ANALOG.ToString()) as string);
            items.Add(Application.Current.TryFindResource(RecMode.DIGITAL.ToString()) as string);
        }
    }
}
