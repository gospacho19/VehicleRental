﻿// <auto-generated />
using System;
using LuxuryCarRental.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace LuxuryCarRental.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250601140928_LoginToken")]
    partial class LoginToken
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.5");

            modelBuilder.Entity("LuxuryCarRental.Models.Basket", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CustomerId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.ToTable("Baskets");
                });

            modelBuilder.Entity("LuxuryCarRental.Models.Card", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CardNumber")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("TEXT");

                    b.Property<int>("CustomerId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Cvv")
                        .IsRequired()
                        .HasMaxLength(4)
                        .HasColumnType("TEXT");

                    b.Property<int>("ExpiryMonth")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ExpiryYear")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Nickname")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.ToTable("Cards");
                });

            modelBuilder.Entity("LuxuryCarRental.Models.CartItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BasketId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("VehicleId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("BasketId");

                    b.HasIndex("VehicleId");

                    b.ToTable("CartItems");
                });

            modelBuilder.Entity("LuxuryCarRental.Models.Customer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("DriverLicenseNumber")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsBlacklisted")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("PasswordSalt")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("RememberMe")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("LuxuryCarRental.Models.Rental", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CustomerId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<int>("VehicleId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.HasIndex("VehicleId");

                    b.ToTable("Rentals");
                });

            modelBuilder.Entity("LuxuryCarRental.Models.Vehicle", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ImagePath")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<int>("VehicleType")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Vehicles");

                    b.HasDiscriminator<int>("VehicleType");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("LuxuryCarRental.Models.Car", b =>
                {
                    b.HasBaseType("LuxuryCarRental.Models.Vehicle");

                    b.Property<string>("Make")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Year")
                        .HasColumnType("INTEGER");

                    b.HasDiscriminator().HasValue(0);
                });

            modelBuilder.Entity("LuxuryCarRental.Models.Motorcycle", b =>
                {
                    b.HasBaseType("LuxuryCarRental.Models.Vehicle");

                    b.Property<int>("EngineCapacityCc")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("HasSidecar")
                        .HasColumnType("INTEGER");

                    b.HasDiscriminator().HasValue(1);
                });

            modelBuilder.Entity("LuxuryCarRental.Models.Yacht", b =>
                {
                    b.HasBaseType("LuxuryCarRental.Models.Vehicle");

                    b.Property<int>("CabinCount")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("LengthInMeters")
                        .HasColumnType("TEXT");

                    b.HasDiscriminator().HasValue(2);
                });

            modelBuilder.Entity("LuxuryCarRental.Models.LuxuryCar", b =>
                {
                    b.HasBaseType("LuxuryCarRental.Models.Car");

                    b.Property<bool>("IncludesChauffeur")
                        .HasColumnType("INTEGER");

                    b.Property<string>("OptionalFeatures")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<decimal>("SecurityDeposit")
                        .HasColumnType("TEXT");

                    b.HasDiscriminator().HasValue(3);
                });

            modelBuilder.Entity("LuxuryCarRental.Models.Basket", b =>
                {
                    b.HasOne("LuxuryCarRental.Models.Customer", "Customer")
                        .WithMany()
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("LuxuryCarRental.Models.Card", b =>
                {
                    b.HasOne("LuxuryCarRental.Models.Customer", "Customer")
                        .WithMany("Cards")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("LuxuryCarRental.Models.CartItem", b =>
                {
                    b.HasOne("LuxuryCarRental.Models.Basket", "Basket")
                        .WithMany("Items")
                        .HasForeignKey("BasketId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("LuxuryCarRental.Models.Vehicle", "Vehicle")
                        .WithMany()
                        .HasForeignKey("VehicleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("LuxuryCarRental.Models.Money", "Subtotal", b1 =>
                        {
                            b1.Property<int>("CartItemId")
                                .HasColumnType("INTEGER");

                            b1.Property<decimal>("Amount")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Currency")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.HasKey("CartItemId");

                            b1.ToTable("CartItems");

                            b1.WithOwner()
                                .HasForeignKey("CartItemId");
                        });

                    b.Navigation("Basket");

                    b.Navigation("Subtotal")
                        .IsRequired();

                    b.Navigation("Vehicle");
                });

            modelBuilder.Entity("LuxuryCarRental.Models.Customer", b =>
                {
                    b.OwnsOne("LuxuryCarRental.Models.ContactInfo", "Contact", b1 =>
                        {
                            b1.Property<int>("CustomerId")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("Email")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<string>("Phone")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.HasKey("CustomerId");

                            b1.ToTable("Customers");

                            b1.WithOwner()
                                .HasForeignKey("CustomerId");
                        });

                    b.Navigation("Contact")
                        .IsRequired();
                });

            modelBuilder.Entity("LuxuryCarRental.Models.Rental", b =>
                {
                    b.HasOne("LuxuryCarRental.Models.Customer", "Customer")
                        .WithMany("Rentals")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("LuxuryCarRental.Models.Vehicle", "Vehicle")
                        .WithMany()
                        .HasForeignKey("VehicleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("LuxuryCarRental.Models.Money", "TotalCost", b1 =>
                        {
                            b1.Property<int>("RentalId")
                                .HasColumnType("INTEGER");

                            b1.Property<decimal>("Amount")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Currency")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.HasKey("RentalId");

                            b1.ToTable("Rentals");

                            b1.WithOwner()
                                .HasForeignKey("RentalId");
                        });

                    b.Navigation("Customer");

                    b.Navigation("TotalCost")
                        .IsRequired();

                    b.Navigation("Vehicle");
                });

            modelBuilder.Entity("LuxuryCarRental.Models.Vehicle", b =>
                {
                    b.OwnsOne("LuxuryCarRental.Models.Money", "DailyRate", b1 =>
                        {
                            b1.Property<int>("VehicleId")
                                .HasColumnType("INTEGER");

                            b1.Property<decimal>("Amount")
                                .HasColumnType("TEXT")
                                .HasColumnName("DailyRate");

                            b1.Property<string>("Currency")
                                .IsRequired()
                                .HasColumnType("TEXT")
                                .HasColumnName("Currency");

                            b1.HasKey("VehicleId");

                            b1.ToTable("Vehicles");

                            b1.WithOwner()
                                .HasForeignKey("VehicleId");
                        });

                    b.Navigation("DailyRate")
                        .IsRequired();
                });

            modelBuilder.Entity("LuxuryCarRental.Models.Basket", b =>
                {
                    b.Navigation("Items");
                });

            modelBuilder.Entity("LuxuryCarRental.Models.Customer", b =>
                {
                    b.Navigation("Cards");

                    b.Navigation("Rentals");
                });
#pragma warning restore 612, 618
        }
    }
}
