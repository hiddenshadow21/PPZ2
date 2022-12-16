using MotionSystems;
using System;
using System.Runtime.InteropServices;

// Ta klasa została napisana przez praktykanta TORUN Technologies, aby możliwie uprościć korzystanie z platformy z poziomu kodu.

/*
	SPOSÓB UŻYCIA TEJ KLASY - INSTRUKCJA
	
	// Pozyskiwanie nowego obiektu reprezentującego platformę:
	MotionSystemsPlatform my_platform = MotionSystemsPlatform.GetAnInstanceOfThePlatform();
	
	// Po skonstruowaniu obiektu trzeba mu obowiązkowo wybrać/nadać tryb pracy poprzez wywołanie .SetWorkingMode(bool)
	my_platform.SetWorkingMode(true);
	// Działanie tego argumentu typu bool jest opisane w komentarzu niżej w tym pliku, tuż przed definicją metody SetWorkingMode.
	// Warto to zrobić już przed rozpoczęciem gry, ponieważ wewnątrz SetWorkingMode jest 3-sekundowe wywołanie funkcji sleep(), aby platforma miała czas się połączyć.
	
	// Kolejne kroki nie są już jednorazowymi krokami początkowymi, lecz można je wykonywać wielokrotnie.
	
	// Platforma jest domyślnie zapauzowana i co za tym idzie nie będzie przyjmowała poleceń ruchu, więc musimy ją najpierw odpauzować:
	// W tym celu, na przykład gdy zaczynamy nową grę - jednorazowo na początku każdej rozgrywki wykonujemy:
	my_platform.StartThePlatform();
	
	// Gdy Platforma jest odpauzowana, to należy jej w miarę regularnie (z częstotliwością co najmniej
	// kilkunastu razy na sekundę, chociaż lepiej wyższą) np. co jedną klatkę wysyłać polecenie ruchu za pomocą jednej z dwóch dostępnych do tego metod tej klasy.
	// Pomimo że istnieją do tego celu dwie metody, klasa pozwoli użyć tylko jednej z nich w zależności od tego, co wpisano jako argument przy wywołaniu SetWorkingMode(bool).
	// Należy zatem często i regularnie wykonywać jedną z tych linii:
	my_platform.OrderThePlatformToMoveBasedOnTelemetry(...); // jeśli tryb pracy ustawiony był na FALSE
	my_platform.OrderThePlatformToReachNewPosition(...); // jeśli tryb pracy ustawiony był na TRUE
	Wymagane parametry formalne/argumenty tych metod należy sprawdzić bezpośrednio w ich definicjach.
	
	// Gdy gra zakończona (czyli platforma ma przestać się ruszać, ponieważ nie będziemy na razie przez jakiś czas wysyłać do niej poleceń ruchu):
	my_platform.StopThePlatform();
	
	// Oczywiście po zapauzowaniu platformy metodą StopThePlatform można ją znowu odpauzować poprzez StartThePlatform i tak wielokrotnie - i nie wymaga to oczywiście tworzenia obiektu platformy od nowa itp.
	// Ciekawostka: Gdy aplikacja jest zamykana, Garbage Collector oczywiście wywoła destruktor m. in. na obiekcie platformy. Jeśli w takim momencie nie była ona zatrzymana, jej destruktor sam zadba o wywołanie StopThePlatform.
*/

public class MotionSystemsPlatform
{
	private static MotionSystemsPlatform singletonPlatform = null;

	private bool mode;
	private bool isPaused;
	private bool everythingLocked;
	private bool isThePlatformObjectCompletelyNew;
	private bool hasModeBeenSetAlready;
	private ForceSeatMI m_fsmi; // obiekt platformy z warstwy niższej
	private FSMI_TopTablePositionPhysical pos; // pozycja dla trybu Top Table Positioning, czyli mode=true
											   // private FSMI_Telemetry telemetry;
	private FSMI_TelemetryACE telemetry; // telemetria - odpowiednik pozycji ale dla drugiego trybu

	static MotionSystemsPlatform()
	{
		singletonPlatform = null;
	}

	private MotionSystemsPlatform()
	{
		everythingLocked = false;
		mode = true;
		isPaused = true;
		isThePlatformObjectCompletelyNew = true;
		hasModeBeenSetAlready = false;

		pos = new FSMI_TopTablePositionPhysical();
		pos.structSize = (byte)Marshal.SizeOf(pos);
		// pos.structSize = sizeof(pos);
		pos.mask = (uint)0;
		pos.mask = FSMI_POS_BIT.STATE | FSMI_POS_BIT.POSITION | FSMI_POS_BIT.MAX_SPEED;

		/* PREPAROWANIE RĘCZNIE dla typu FSMI_Telemetry a nie FSMI_TelemetryACE:
		telemetry = new FSMI_Telemetry();
		telemetry.structSize = (byte)Marshal.SizeOf(telemetry);
		// telemetry.structSize = sizeof(telemetry);
		telemetry.mask = (uint)0;
		telemetry.mask = 
			FSMI_TEL_BIT.STATE | 
			FSMI_TEL_BIT.SPEED |
			FSMI_TEL_BIT.YAW_PITCH_ROLL |
			FSMI_TEL_BIT.YAW_PITCH_ROLL_ACCELERATION |
			FSMI_TEL_BIT.YAW_PITCH_ROLL_SPEED |
			FSMI_TEL_BIT.SWAY_HEAVE_SURGE_ACCELERATION |
			FSMI_TEL_BIT.SWAY_HEAVE_SURGE_SPEED;
		*/
		telemetry = FSMI_TelemetryACE.Prepare(); // w przypadku FSMI_TelemetryACE można po prostu skorzystać z gotowej metody do preparowania/przygotowania

		m_fsmi = new ForceSeatMI();
		if (!m_fsmi.IsLoaded())
		{
			everythingLocked = true;
			throw new Exception("Exception: Nie udało się poprawnie załadować/uzyskać obiektu, który miał udostępniać platformę.");
		}
	}

	public static MotionSystemsPlatform GetAnInstanceOfThePlatform()
	{
		if (singletonPlatform == null)
		{
			singletonPlatform = new MotionSystemsPlatform();
		}
		return singletonPlatform;
	}

	public static void ForceTheCurrentInstanceToBeRemoved()
	{
		(MotionSystemsPlatform.GetAnInstanceOfThePlatform()).PrepareToRemoveTheCurrentInstance();
		singletonPlatform = null;
	}

	private void PrepareToRemoveTheCurrentInstance()
	{
		if (!everythingLocked)
		{
			this.StopThePlatform();
			if (m_fsmi.IsLoaded()) m_fsmi.Dispose();
		}
	}

	~MotionSystemsPlatform()
	{
		this.PrepareToRemoveTheCurrentInstance();
	}

	// mode=true -> Top Table Positioning (precise control over the platform position from your code)
	// mode=false -> Vehicle Telemetry (useful only for: racing games, simulating forces etc.)
	public void SetWorkingMode(bool wishedMode)
	{
		if (isThePlatformObjectCompletelyNew)
		{
			isThePlatformObjectCompletelyNew = false;
			mode = wishedMode;
			hasModeBeenSetAlready = true;
			if (mode)
			{
				m_fsmi.ActivateProfile("SDK - Positioning");
			}
			else
			{
				m_fsmi.ActivateProfile("SDK - Vehicle Telemetry ACE");
			}
			System.Threading.Thread.Sleep(3000); // waiting for the platform to connect...
		}
		else
		{
			throw new Exception("Exception: Nie można zmienić trybu na nieświeżym obiekcie platformy. Wykonaj ten krok wcześniej lub zresetuj obiekt metodą statyczną ForceTheCurrentInstanceToBeRemoved i pozyskaj jeszcze raz poprzez GetAnInstanceOfThePlatform.");
		}
	}

	public void StartThePlatform()
	{
		isThePlatformObjectCompletelyNew = false;
		if (isPaused && (!everythingLocked) && hasModeBeenSetAlready)
		{
			if (m_fsmi.IsLoaded())
			{
				isPaused = false;
				m_fsmi.BeginMotionControl();
			}
			else
			{
				everythingLocked = true;
				isPaused = true;
				throw new Exception("Exception: W trakcie działania programu z niewyjaśnionych przyczyn utracono obiekt, który miał udostępniać platformę. Plik ForceSeatMI.cs stworzony przez MotionSystems nie spełnił swojej roli.");
			}
		}
		else
		{
			if (!isPaused)
			{
				throw new Exception("Exception: Nie można odpauzować platformy, gdy już jest odpauzowana!");
			}
			if (everythingLocked)
			{
				throw new Exception("Exception: Nie można pracować na obiekcie platformy, ponieważ celowo został on zablokowany - czy na pewno masz zainstalowane ForceSeatPM?");
			}
			if (!hasModeBeenSetAlready)
			{
				throw new Exception("Exception: Podjęto próbę odpauzowania platformy, lecz przedtem nie został ustawiony żaden tryb pracy. Upewnij się, że wcześniej wywołujesz metodę SetWorkingMode.");
			}
		}
	}

	public void StopThePlatform()
	{
		if (!isPaused)
		{
			isPaused = true;
			m_fsmi.EndMotionControl();
		}
		else
		{
			throw new Exception("Exception: Nie można zapauzować platformy, gdy już jest zapauzowana!");
		}
	}

	// ta metoda wymaga podania konkretnej nowej pozycji docelowej, a nie żadnych przesunięć w stylu "delta x", ani nie prędkości czy przyspieszeń
	public void OrderThePlatformToReachNewPosition(ushort maxPlatformSpeedAllowedByYou_ushort_from_0_to_65535, float położeniePion_float_wMilimetrach, float pochylenieDoTyłu_float_wRadianach, float pochylenieWPrawo_float_wRadianach)
	{
		if (isPaused) { throw new Exception("Exception: Wydano platformie rozkaz ruchu podczas gdy ona jest w trybie pauzy!!!"); }
		else
		{
			if (!mode) { throw new Exception("Exception: Wydano platformie rozkaz niepasujacy do tego trybu pracy!!!"); }
			else
			{
				pos.state = FSMI_State.NO_PAUSE; // nadpisuje to samo bez sensu, ale to praktycznie zerowy koszt wydajnosci, a przynajmniej raz to trzeba ustawic a moze wiecej niz raz, wiec robimy tak jak bylo w dostarczonych przykladach
				pos.maxSpeed = maxPlatformSpeedAllowedByYou_ushort_from_0_to_65535; // 0 -> slowest, (65535 = 100%) -> fastest

				pos.pitch = pochylenieDoTyłu_float_wRadianach; // in rad
				pos.roll = pochylenieWPrawo_float_wRadianach; // in rad 
				pos.yaw = 0.0f; // in rad
				pos.sway = 0.0f; // in mm 
				pos.surge = 0.0f; // in mm 
				pos.heave = położeniePion_float_wMilimetrach; // in mm

				m_fsmi.SendTopTablePosPhy(ref pos);
			}
		}
	}

	// Autor poniższej metody jedynie przekazuje argumenty dalej, nie znając ich znaczenia i roli.
	// Załączony z tą klasą mój przykładowy projekt w Unity powodował przy użyciu tej metody obserwowalny ruch platformy, jednak nie jest jasne za co odpowiadały konkretne parametry.
	// Prawdopodobnie należy podawać dane fizyczne dotyczące np. samochodu a one będą przez niższą warstwę przekładane na ruchy platformy mające poprzez pochylanie udawać np. działanie siły odśrodkowej podczas skrętu pojazdem.
	public void OrderThePlatformToMoveBasedOnTelemetry(
		float bodyAngularVelocityYaw,
		float bodyAngularVelocityPitch,
		float bodyAngularVelocityRoll,
		float bodyLinearAccelerationForward,
		float bodyLinearAccelerationUpward,
		float bodyLinearAccelerationRight,
		float extraRotationYaw,
		float extraRotationPitch,
		float extraRotationRoll,
		float extraTranslationRight,
		float extraTranslationUpward,
		float extraTranslationForward
		)
	/*
	float vehicleSpeed_MetersPerSec,
	float yaw_rad,
	float vehiclePitch_rad,
	float vehicleRoll_rad,
	float swaySpeed,
	float heaveSpeed,
	float surgeSpeed,
	float swayAcceleration,
	float heaveAcceleration,
	float surgeAcceleration,
	float yawSpeed,
	float pitchSpeed,
	float rollSpeed,
	float yawAcceleration,
	float pitchAcceleration,
	float rollAcceleration
	)
	*/
	{
		if (isPaused) { throw new Exception("Exception: Wydano platformie rozkaz ruchu podczas gdy ona jest w trybie pauzy!!!"); }
		else
		{
			if (mode) { throw new Exception("Exception: Wydano platformie rozkaz niepasujący do tego trybu pracy!!!"); }
			else
			{
				telemetry.state = FSMI_State.NO_PAUSE;

				/*
				telemetry.speed = vehicleSpeed_MetersPerSec;
				
				telemetry.yaw = yaw_rad; // yaw in radians
				telemetry.pitch = vehiclePitch_rad; // vehicle pitch in radians
				telemetry.roll = vehicleRoll_rad; // vehicle roll in radians

				telemetry.swaySpeed = swaySpeed;  // m/s
				telemetry.heaveSpeed = heaveSpeed; // m/s
				telemetry.surgeSpeed = surgeSpeed; // m/s

				telemetry.swayAcceleration = swayAcceleration;  // m/s^2
				telemetry.heaveAcceleration = heaveAcceleration; // m/s^2
				telemetry.surgeAcceleration = surgeAcceleration; // m/s^2

				telemetry.yawSpeed = yawSpeed;   // radians/s
				telemetry.pitchSpeed = pitchSpeed; // radians/s
				telemetry.rollSpeed = rollSpeed;  // radians/s
				
				telemetry.yawAcceleration = yawAcceleration;   // radians/s^2
				telemetry.pitchAcceleration = pitchAcceleration; // radians/s^2
				telemetry.rollAcceleration = rollAcceleration;  // radians/s^2
				*/

				telemetry.bodyAngularVelocity[0].yaw = bodyAngularVelocityYaw;
				telemetry.bodyAngularVelocity[0].pitch = bodyAngularVelocityPitch;
				telemetry.bodyAngularVelocity[0].roll = bodyAngularVelocityRoll;
				telemetry.bodyLinearAcceleration[0].forward = bodyLinearAccelerationForward;
				telemetry.bodyLinearAcceleration[0].upward = bodyLinearAccelerationUpward;
				telemetry.bodyLinearAcceleration[0].right = bodyLinearAccelerationRight;
				telemetry.extraRotation[0].yaw = extraRotationYaw;
				telemetry.extraRotation[0].pitch = extraRotationPitch;
				telemetry.extraRotation[0].roll = extraRotationRoll;
				telemetry.extraTranslation[0].right = extraTranslationRight;
				telemetry.extraTranslation[0].upward = extraTranslationUpward;
				telemetry.extraTranslation[0].forward = extraTranslationForward;

				// m_fsmi.SendTelemetry(ref telemetry);
				m_fsmi.SendTelemetryACE(ref telemetry);
			}
		}
	}
}

// Możliwość rozwoju (nie wydaje się to być potrzebne): W razie czego przykład TopTablePos_CS pokazuje jak zapytać platformę o jej stan (przy użyciu metod samego Motion Interface), czyli: "Czy jest zpauzowana?", "Czy nie zerwalo polaczenia?" itp. - interesujacy jest wówczas tylko obiekt platformInfo. Obecne rozwiązanie i tak ma już metody wykrywania problemów (wywołania .IsLoaded()) a oprócz tego sam Platform Manager też zajmuje się takimi rzeczami jak przegrzewanie, platform disconnections etc., więc my nie musimy.
// Kolejna możliwość rozwoju: Przyklad TopTablePos_CS pokazuje jak na bazie dźwięku generować efekty specjalne (drgania platformy etc.).