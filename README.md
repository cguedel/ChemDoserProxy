# ChemDoserProxy
Proxy tool converting TCP stream from Aseko Doser systems to HTTP API

## Disclaimer
Everything the proxy does was reverse engineered from observing the values sent to pool.aseko.com.
Note that connection is unencrypted.

**Due to the nature of the process, some things could be wrong. No responsibility will be accepted
for any issues resulting of the use of this tool.**  

## OpenHab
This can be used to integrate the Aseko Doser systems with OpenHab 
by connecting via HTTP binding.

Following steps are required:

### Connecting Doser to Docker container
- Copy [docker-compose.yaml](./Docker/docker-compose.yaml) to a folder of your choice
- Adapt environment configuration to your needs
- Go to your Aseko doser web interface (it's a simple RS232 to IP converter, identifies as `USR-K5` in DHCP)
  - Default username/password is `admin`/`admin`
  - Select serial port in the menu and change remote server address to the IP of the machine running the Docker container.
- `docker compose up` to start the container
- Check `http://<your-ip>:47525/state` and verify values match what the Aseko app on your phone/and or the screen on the doser is showing

### Configure OpenHab
- Install the `http` binding, if not already installed
- Create `aseko.things` file:
```config
Thing http:url:doser-proxy "Aseko Doser" [
    baseURL="http://<your-ip>:47525",
    refresh=10,
    commandMethod="POST"]
    {
        Channels: 
            Type number : WaterTemperature  "Water temperature" [ stateExtension="state", stateTransformation="JSONPATH:$.state.waterTemp", mode="READONLY" ]
            Type number : clFree            "cl Free"           [ stateExtension="state", stateTransformation="JSONPATH:$.state.clFree", mode="READONLY" ]
            Type number : clFreeMv          "cl Free (mV)"      [ stateExtension="state", stateTransformation="JSONPATH:$.state.clFreeMv", mode="READONLY" ]
            Type number : pH                "pH"                [ stateExtension="state", stateTransformation="JSONPATH:$.state.pH", mode="READONLY" ]

            Type number : LevelChlorPure    "Level ChlorPure"   [ stateExtension="state", stateTransformation="JSONPATH:$.levels.chlorPure", mode="READONLY" ]
            Type number : LevelpHMinus      "Level pH Minus"    [ stateExtension="state", stateTransformation="JSONPATH:$.levels.pHMinus", mode="READONLY" ]
            Type number : LevelpHPlus       "Level pH Plus"     [ stateExtension="state", stateTransformation="JSONPATH:$.levels.pHPlus", mode="READONLY" ]
            Type number : LevelFlocPlusC    "Level Floc+C"      [ stateExtension="state", stateTransformation="JSONPATH:$.levels.flocPlusC", mode="READONLY" ]

            Type switch : FillChlorPure     "Refill ChlorPure"  [ commandExtension="chemical/%2$s/fill", onValue="ChlorPure", offValue="noop", mode="WRITEONLY" ]
            Type switch : FillpHMinus       "Refill pH Minus"   [ commandExtension="chemical/%2$s/fill", onValue="pHMinus", offValue="noop", mode="WRITEONLY" ]
            Type switch : FillpHPlus        "Refill pH Plus"    [ commandExtension="chemical/%2$s/fill", onValue="pHPlus", offValue="noop", mode="WRITEONLY" ]
            Type switch : FillFlocPlusC     "Refill Floc+C"     [ commandExtension="chemical/%2$s/fill", onValue="FlocPlusC", offValue="noop", mode="WRITEONLY" ]
    }
```
- Create `aseko.items` file:
```config
Number      pH               "pH Value [%.2f]"                      {channel="http:url:doser-proxy:pH"}
Number      clFree           "Concentration Chlorine [%.2f mg/l]"   {channel="http:url:doser-proxy:clFree"}
Number      clFreeMv         "Measurement Chlorine [%d mV]"         {channel="http:url:doser-proxy:clFreeMv"}
Number      temp             "Temperature [%.1f °C]"                {channel="http:url:doser-proxy:WaterTemperature"}

Number      level_ChlorPure  "Level Chlor Pure [%d ml]"             {channel="http:url:doser-proxy:LevelChlorPure"}
Number      level_pHMinus    "Level pH Minus [%d ml]"               {channel="http:url:doser-proxy:LevelpHMinus"}
Number      level_pHPlus     "Level pH Plus [%d ml]"                {channel="http:url:doser-proxy:LevelpHPlus"}
Number      level_FlocPlusC  "Level Floc+C [%d ml]"                 {channel="http:url:doser-proxy:LevelFlocPlusC"}

Switch      refill_ChlorPure "Refilled Chlor Pure"                  {channel="http:url:doser-proxy:FillChlorPure"}
Switch      refill_pHMinus   "Refilled pH Minus"                    {channel="http:url:doser-proxy:FillpHMinus"}
Switch      refill_pHPlus    "Refilled pH Plus"                     {channel="http:url:doser-proxy:FillpHPlus"}
Switch      refill_FlocPlusC "Refilled Floc+C"                      {channel="http:url:doser-proxy:FillFlocPlusC"}
```
