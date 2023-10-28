using AspNetCore.Identity.Mongo.Model;
using TripleTriad.Interfaces;

namespace TripleTriad.Users;
public sealed class User : MongoUser<string>, IEntity<string>
{
}
