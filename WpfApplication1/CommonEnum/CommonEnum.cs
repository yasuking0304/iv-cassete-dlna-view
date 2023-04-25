using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace View.CommonEnum {

    public enum GCODE_SET {
        NULL = -1,
        G0   =  0,
        G1,
        G2,
        G3
    }

 	public enum CODE_SET {
		NULL = -1,				// 不明なグラフィックセット(非対応)
		ALPHANUMERIC = 0,   	// Alphanumeric
		HIRAGANA,				// Hiragana
		KATAKANA,				// Katakana
		KANJI,  				// Kanji
		MOSAIC_A,				// Mosaic A
		MOSAIC_B,				// Mosaic B
		MOSAIC_C,				// Mosaic C
		MOSAIC_D,				// Mosaic D
		PROP_ALPHANUMERIC,		// Proportional Alphanumeric
		PROP_HIRAGANA,			// Proportional Hiragana
		PROP_KATAKANA,			// Proportional Katakana
		JIS_X0201_KATAKANA,	    // JIS X 0201 Katakana
		JIS_KANJI_PLANE_1,		// JIS compatible Kanji Plane 1
		JIS_KANJI_PLANE_2,		// JIS compatible Kanji Plane 2
		ADDITIONAL_SYMBOLS		// Additional symbols
	}

    public enum EventStatus {
        ONSTART,
        ONSTART_GENRE,          // upnp Container
        ONDATA,                 //
        ONDEL_DEVICE,           // delete device(イベント専用)
        ONADD_DEVICE,           // add device(イベント専用)
        ONDATA_FOLDER,          // upnp Container
        ONDATA_ITEM,            // upnp item
        ONDATA_GENRE,           // upnp Container
        ONEND,
        ONEND_DEVICE,
        ONEND_GENRE,            // upnp Container
        ON_CHKLIVETUNER,        //
        ON_LIVETUNER,           //
        ONERROR,
    }

    public enum RecMode {
        ALL,
        DIGITAL,
        TB,
        BS,
        CS,
        ANALOG,
    }

    public enum A_ARG_TYPE {
        ObjectID       = 0,
        BrowseFlag     = 1,
        SearchCriteria = 1,
        Filter         = 2,
        Index          = 3,
        Count          = 4,
        SortCriteria   = 5,
    }

    [Flags]
    public enum X_Record {
        NONE                    = 0x0000,
        SCHEDULEDRECORDING1     = 0x0001,
        SCHEDULEDRECORDING2     = 0x0002,
        X_SCHEDULEDRECORDING    = 0x0004,
        X_SCHEDULEDRECORDINGEXT = 0x0008,
    }

    public enum EXEC_MODE {
        NONE,
        IVDR,
        DLNA,
        DLNA_CONTENTS,
        WAIT,
    }

    public enum WM : uint {
        DEVMODECHANGE = 0x1e, 
        DEVICECHANGE  = 0x0219,
        SYSCOMMAND    = 0x112,
        APP           = 0x8000,
        APP_IVDR_CHK  = 0xB021,
    }

    public enum DBT {
        DEVTYPDEVICEINTERFACE   = 0x0005,
        DEVNODES_CHANGED        = 0x0007,
        DEVICEARRIVAL           = 0x8000,
        DEVICEQUERYREMOVE       = 0x8001,
        DEVICEQUERYREMOVEFAILED = 0x8002,
        DEVICEREMOVEPENDING     = 0x8003,
        DEVICEREMOVECOMPLETE    = 0x8004,
    }

    // コールバック種別
    public enum CallbackCommand {
        PROGRESS,
        DONEWITHSTRUCTURE,
        UNKNOWN2,
        UNKNOWN3,
        UNKNOWN4,
        UNKNOWN5,
        INSUFFICIENTRIGHTS,
        UNKNOWN7,
        DISKLOCKEDFORACCESS, //UNKNOWN8,
        UNKNOWN9,
        UNKNOWNA,
        DONE,
        UNKNOWNC,
        UNKNOWND,
        OUTPUT,
        STRUCTUREPROGRESS
    }
    [Flags]
    public enum DlnaSupport : ulong {
        NONE                    = 0x00000000,
        DLNACAPAVUPLOAD         = 0x00000001,
        DLNACAPDTCPMOVE         = 0x00000002,
        DLNACAPDTCPCOPY         = 0x00000004,
        JLABSCAPMOVE            = 0x00000008,
        JLABSCAPUPLOADREC       = 0x00000010,
        SPTVCAPREC              = 0x00000020,
        SPTVCAPMOVE             = 0x00000040,
        LIVETUNER               = 0x00000080,
        CHKLIVETUNER            = 0x00000100,
        REGZACAPMOVE            = 0x00000200,
        NODLNA                  = 0x80000000,
    }
}
