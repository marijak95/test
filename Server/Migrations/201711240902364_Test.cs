namespace Server
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Test : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Requests",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StartDate = c.String(nullable: false),
                        EndDate = c.String(nullable: false),
                        RequestManagers = c.String(),
                        UserId = c.Int(nullable: false),
                        Approved = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Teams",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TeamName = c.String(),
                        BossId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Username = c.String(nullable: false),
                        FirstName = c.String(nullable: false),
                        RequireSafeLogin = c.Int(nullable: false),
                        LastName = c.String(nullable: false),
                        Password = c.String(nullable: false),
                        QuestionOne = c.String(nullable: false),
                        QuestionTwo = c.String(nullable: false),
                        AnswerOne = c.String(nullable: false),
                        AnswerTwo = c.String(nullable: false),
                        BossId = c.Int(nullable: false),
                        Team = c.Int(nullable: false),
                        Group = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Users");
            DropTable("dbo.Teams");
            DropTable("dbo.Requests");
        }
    }
}
