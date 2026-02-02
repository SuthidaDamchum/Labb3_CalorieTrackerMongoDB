# 🍎 Calorie Tracker App 
This is a school project made to learn how to work with MongoDB using the MongoDB.Driver in a C# application.
I chose to build a Calorie Tracker because I already track my daily calories in Excel. Creating my own app felt useful and motivating, and it made the project more personal and practical.
The application connects to a local MongoDB instance. When it runs for the first time, it creates its own database and adds demo data if the database does not already exist.

## Learning Goals 

This project focuses on learning and practicing:
* MongoDB
* MongoDB Compass
* MongoDB.Driver
* Database seeding
* CRUD operations
* Asynchronous database communication

## Application Features

The app consists of three main views:

1. Daily Log
* View current date and date history
* Set a weekly calorie goal (one goal per week)
* See a daily summary comparing goal vs actual calories
* View a list of foods eaten during the day
* Increase or decrease the amount of a food item
* Delete food items from the daily log

2. Food List
* View all available food items
* Full CRUD functionality 
* Add selected food items directly to today’s daily log

3. Weekly Summary
* View weekly history
* See weekly food intake
* Compare macro goals with actual results for the week

## Notes
This project helped me understand how a real application interacts with a NoSQL database and how to structure data for daily and weekly tracking. 
It also gave me hands-on experience building something I can actually use in everyday life.

## 🖼️ Screenshots
### Daily Log
This view shows the food items logged for the selected day, including calories and macronutrients. The amount can be adjusted in this view.
![DailyLogView](https://github.com/user-attachments/assets/244de8f5-525d-4591-a1ef-1e6bd9664191)

### Food List
This view allows the user to add, edit, and delete food items stored in the MongoDB database.
![FoodView](https://github.com/user-attachments/assets/d35c4b3c-3acf-4c70-aec0-a2dd884b8160)

### Weekly Summary 
This view shows daily calories and macros for the selected week, with buttons to see the previous and next weeks.
![WeeklyView](https://github.com/user-attachments/assets/456771ce-b8b2-4b5f-825d-8fc0e0fc854b)

### Add / Edit Food dialog
Dialog for adding and editing food items.
![Add-Edit food dialog](https://github.com/user-attachments/assets/131b4ff9-ae86-4fca-ab65-4ff4437aad8b)




