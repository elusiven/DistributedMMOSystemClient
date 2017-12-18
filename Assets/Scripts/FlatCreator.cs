using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using elusivenWorks;
using FlatBuffers;

namespace Assets.Scripts
{
    public static class FlatCreator
    {
        public static Offset<PlayerInfo> CreatePlayerInfo(FlatBufferBuilder fbb, string id, float pX, float pY, float pZ, float rX, float rY, float rZ, float rW)
        {
            var _id = fbb.CreateString(id);

            PlayerInfo.StartPlayerInfo(fbb);
            PlayerInfo.AddId(fbb, _id);
            PlayerInfo.AddPos(fbb, Vec3.CreateVec3(fbb, pX, pY, pZ));
            PlayerInfo.AddRot(fbb, Qua.CreateQua(fbb, rX, rY, rZ, rW));
            return PlayerInfo.EndPlayerInfo(fbb);
        }
    }
}
